using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

// Builds the student's avatar as inline SVG.
//
// Every student starts with the same neutral, hairless base character, so it
// works as a default for anyone. Shop items whose ItemPath starts with
// "avatar:" add cosmetic layers on top (hair, hats, earrings, glasses,
// clothes). Only one item per Type can be equipped at a time (enforced in
// ItemShop.SetEquip).
//
// Web Application: place this file in the PROJECT ROOT (not App_Code).
public static class Avatar
{
    // ---------- data: which avatar layers does this student have equipped ----------
    public static List<string> GetEquippedKeys(string connStr, int sid)
    {
        List<string> keys = new List<string>();
        using (SqlConnection cn = new SqlConnection(connStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT i.ItemPath FROM Inventory inv " +
            "JOIN Item i ON i.ItemID = inv.ItemID " +
            "WHERE inv.StudentID = @s AND inv.EquipStatus = 'Equipped' " +
            "AND i.ItemPath LIKE 'avatar:%'", cn))
        {
            cmd.Parameters.AddWithValue("@s", sid);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read())
                    keys.Add(r[0].ToString());
        }
        return keys;
    }

    // ---------- drawing ----------
    private const string Skin = "#d9a679";      // neutral warm skin tone
    private const string SkinDark = "#c28e63";  // neck shade
    private const string Ink = "#33302c";       // eyes / mouth / glasses

    // The base character: bald, simple face, plain grey tee. Deliberately neutral.
    private const string BaseBody =
        "<rect x='86' y='108' width='28' height='48' rx='12' fill='" + SkinDark + "'/>" +          // neck
        "<path d='M52 240 V198 Q52 152 100 150 Q148 152 148 198 V240 Z' fill='#7d8590'/>" +        // torso (default tee)
        "<path d='M86 150 Q100 160 114 150 L114 142 H86 Z' fill='" + SkinDark + "'/>" +            // collar skin
        "<circle cx='56' cy='82' r='9' fill='" + Skin + "'/>" +                                    // left ear
        "<circle cx='144' cy='82' r='9' fill='" + Skin + "'/>" +                                   // right ear
        "<circle cx='100' cy='78' r='44' fill='" + Skin + "'/>" +                                  // head
        "<circle cx='84' cy='76' r='4.5' fill='" + Ink + "'/>" +                                   // left eye
        "<circle cx='116' cy='76' r='4.5' fill='" + Ink + "'/>" +                                  // right eye
        "<path d='M88 95 Q100 104 112 95' fill='none' stroke='" + Ink + "' stroke-width='3' stroke-linecap='round'/>"; // smile

    // Cosmetic layers, keyed by the part of ItemPath after "avatar:".
    // The word before the underscore is the slot (hair, hat, ear, glasses, clothes).
    private static readonly Dictionary<string, string> Layers = new Dictionary<string, string>
    {
        // ----- clothes (drawn over the default tee) -----
        { "clothes_hoodie",
            "<path d='M52 240 V198 Q52 152 100 150 Q148 152 148 198 V240 Z' fill='#d8543c'/>" +
            "<path d='M64 178 Q100 202 136 178 Q136 156 100 151 Q64 156 64 178 Z' fill='#b8452f'/>" +
            "<path d='M90 176 v22 M110 176 v22' stroke='#f2e8e4' stroke-width='4' stroke-linecap='round' fill='none'/>" },
        { "clothes_tee",
            "<path d='M52 240 V198 Q52 152 100 150 Q148 152 148 198 V240 Z' fill='#4a7dbf'/>" +
            "<path d='M84 151 Q100 164 116 151 Q108 148 100 148 Q92 148 84 151 Z' fill='#3a6398'/>" },

        // ----- hair -----
        { "hair_short",
            "<path d='M56 76 Q52 30 100 30 Q148 30 144 76 Q142 58 126 49 Q112 42 100 43 Q88 42 74 49 Q58 58 56 76 Z' fill='#3b2f28'/>" },
        { "hair_long",
            "<path d='M54 72 Q50 26 100 26 Q150 26 146 72 L150 134 Q150 152 134 152 L136 96 Q122 44 100 44 Q78 44 64 96 L66 152 Q50 152 50 134 Z' fill='#4a3423'/>" },
        { "hair_curly",
            "<circle cx='66' cy='58' r='16' fill='#2e2a3f'/><circle cx='86' cy='44' r='17' fill='#2e2a3f'/>" +
            "<circle cx='108' cy='40' r='17' fill='#2e2a3f'/><circle cx='128' cy='50' r='16' fill='#2e2a3f'/>" +
            "<circle cx='140' cy='66' r='13' fill='#2e2a3f'/><circle cx='58' cy='72' r='12' fill='#2e2a3f'/>" },

        // ----- earrings -----
        { "ear_stud",
            "<circle cx='55' cy='84' r='3.5' fill='#f7c948'/><circle cx='145' cy='84' r='3.5' fill='#f7c948'/>" },
        { "ear_hoop",
            "<circle cx='56' cy='93' r='6' fill='none' stroke='#f7c948' stroke-width='3'/>" +
            "<circle cx='144' cy='93' r='6' fill='none' stroke='#f7c948' stroke-width='3'/>" },

        // ----- glasses -----
        { "glasses_round",
            "<circle cx='84' cy='76' r='12' fill='none' stroke='" + Ink + "' stroke-width='3'/>" +
            "<circle cx='116' cy='76' r='12' fill='none' stroke='" + Ink + "' stroke-width='3'/>" +
            "<path d='M96 76 h8 M72 73 L58 69 M128 73 L142 69' stroke='" + Ink + "' stroke-width='3' fill='none' stroke-linecap='round'/>" },

        // ----- hats (drawn last, so they sit over hair) -----
        { "hat_cap",
            "<path d='M58 62 Q58 24 100 24 Q142 24 142 62 Z' fill='#d8543c'/>" +
            "<path d='M140 54 Q176 56 178 64 Q178 70 140 66 Z' fill='#b8452f'/>" +
            "<circle cx='100' cy='26' r='5' fill='#b8452f'/>" },
        { "hat_beanie",
            "<path d='M58 66 Q58 22 100 22 Q142 22 142 66 Z' fill='#40603c'/>" +
            "<rect x='55' y='58' width='90' height='14' rx='7' fill='#33502f'/>" +
            "<circle cx='100' cy='20' r='7' fill='#33502f'/>" },
        { "hat_pixel",   // blocky 8-bit cap built from squares
            "<g fill='#7b5cff'>" +
            "<rect x='60' y='48' width='16' height='16'/><rect x='76' y='36' width='16' height='16'/>" +
            "<rect x='92' y='30' width='16' height='16'/><rect x='108' y='36' width='16' height='16'/>" +
            "<rect x='124' y='48' width='16' height='16'/></g>" +
            "<g fill='#5a3fd6'>" +
            "<rect x='76' y='52' width='16' height='10'/><rect x='92' y='46' width='16' height='10'/>" +
            "<rect x='108' y='52' width='16' height='10'/>" +
            "<rect x='138' y='54' width='16' height='10'/><rect x='154' y='58' width='14' height='10'/></g>" },
        { "glasses_neon",  // glowing cyan visor
            "<rect x='68' y='66' width='30' height='20' rx='6' fill='none' stroke='#28e6d5' stroke-width='3.5'/>" +
            "<rect x='102' y='66' width='30' height='20' rx='6' fill='none' stroke='#28e6d5' stroke-width='3.5'/>" +
            "<path d='M98 74 h4 M68 72 L57 68 M132 72 L143 68' stroke='#28e6d5' stroke-width='3.5' fill='none' stroke-linecap='round'/>" +
            "<rect x='68' y='66' width='30' height='20' rx='6' fill='#28e6d5' opacity='.18'/>" +
            "<rect x='102' y='66' width='30' height='20' rx='6' fill='#28e6d5' opacity='.18'/>" },
        { "clothes_coder", // dark coder hoodie with </> on the chest
            "<path d='M52 240 V198 Q52 152 100 150 Q148 152 148 198 V240 Z' fill='#2e3440'/>" +
            "<path d='M64 178 Q100 202 136 178 Q136 156 100 151 Q64 156 64 178 Z' fill='#232a35'/>" +
            "<path d='M90 176 v20 M110 176 v20' stroke='#4c566a' stroke-width='4' stroke-linecap='round' fill='none'/>" +
            "<text x='100' y='222' font-family='Consolas,monospace' font-size='20' font-weight='bold' fill='#28e6d5' text-anchor='middle'>&lt;/&gt;</text>" },
        { "frame_gold",    // decorative golden ring + soft glow behind the head
            "<circle cx='100' cy='78' r='56' fill='#f7c948' opacity='.16'/>" +
            "<circle cx='100' cy='78' r='52' fill='none' stroke='#f7c948' stroke-width='4'/>" +
            "<circle cx='100' cy='78' r='52' fill='none' stroke='#fff3c4' stroke-width='4' stroke-dasharray='4 22' stroke-linecap='round'/>" },
    };

    // Where an UPLOADED image sits for each body part (x/y/width/height inside
    // the 200x240 avatar view box).
    private static readonly Dictionary<string, string> ImageBoxes = new Dictionary<string, string>
    {
        { "frame",   "x='28' y='6'   width='144' height='144'" },
        { "clothes", "x='44' y='145' width='112' height='95'"  },
        { "hair",    "x='44' y='8'   width='112' height='85'"  },
        { "ear",     "x='32' y='66'  width='136' height='48'"  },
        { "glasses", "x='54' y='54'  width='92'  height='44'"  },
        { "hat",     "x='44' y='0'   width='112' height='68'"  },
    };

    // Draw order: frame behind everything, clothes under the chin, then hair,
    // earrings, glasses, and the hat on top.
    private static readonly string[] SlotOrder = { "frame", "clothes", "hair", "ear", "glasses", "hat" };

    // headOnly = true crops to the face (used for the small nav-bar icon).
    public static string Render(IEnumerable<string> keys, bool headOnly)
    {
        Dictionary<string, string> bySlot = new Dictionary<string, string>();
        if (keys != null)
        {
            foreach (string k in keys)
            {
                if (string.IsNullOrEmpty(k) || !k.StartsWith("avatar:")) continue;
                string name = k.Substring("avatar:".Length);

                // admin-uploaded cosmetic: "avatar:img:{slot}:{image path}" - the image
                // is drawn inside a fixed box for that body part.
                if (name.StartsWith("img:"))
                {
                    string[] parts = name.Split(new char[] { ':' }, 3);
                    if (parts.Length == 3 && ImageBoxes.ContainsKey(parts[1]))
                    {
                        string box = ImageBoxes[parts[1]];
                        string href = parts[2].Replace("&", "&amp;").Replace("'", "&apos;").Replace("<", "&lt;");
                        bySlot[parts[1]] =
                            "<image href='" + href + "' " + box +
                            " preserveAspectRatio='xMidYMid meet'/>";
                    }
                    continue;
                }

                if (!Layers.ContainsKey(name)) continue;
                string slot = name.Split('_')[0];
                bySlot[slot] = Layers[name];
            }
        }

        StringBuilder sb = new StringBuilder();
        string viewBox = headOnly ? "40 10 120 120" : "0 0 200 240";
        sb.Append("<svg viewBox='").Append(viewBox)
          .Append("' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='avatar' preserveAspectRatio='xMidYMid meet'>");
        sb.Append(BaseBody);
        foreach (string slot in SlotOrder)
            if (bySlot.ContainsKey(slot))
                sb.Append(bySlot[slot]);
        sb.Append("</svg>");
        return sb.ToString();
    }

    public static string Render(IEnumerable<string> keys)
    {
        return Render(keys, false);
    }
}