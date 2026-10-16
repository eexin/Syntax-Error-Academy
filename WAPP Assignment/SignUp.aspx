<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SignUp.aspx.cs" Inherits="SignUp" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Sign up - Syntax Error Academy</title>
    <link rel="stylesheet" href="Styles/sea-auth.css?v=3" />
</head>
<body class="auth-page">
    <form id="form1" runat="server">

        <div class="auth-brand"><span class="b">&lt;/&gt;</span> SEA</div>
        <div class="auth-tag">Create your account</div>

        <div class="auth-card">
            <h1>Sign up</h1>

            <asp:Label ID="lblError" runat="server" CssClass="auth-error" Visible="false" />

            <div class="field">
                <label>Email</label>
                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" />
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server"
                    ControlToValidate="txtEmail" Display="Dynamic" CssClass="vmsg"
                    ErrorMessage="Email is required." />
                <asp:RegularExpressionValidator ID="revEmail" runat="server"
                    ControlToValidate="txtEmail" Display="Dynamic" CssClass="vmsg"
                    ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                    ErrorMessage="Enter a valid email address." />
            </div>

            <div class="field">
                <label>Password</label>
                <asp:TextBox ID="txtPwd" runat="server" TextMode="Password" />
                <asp:RequiredFieldValidator ID="rfvPwd" runat="server"
                    ControlToValidate="txtPwd" Display="Dynamic" CssClass="vmsg"
                    ErrorMessage="Password is required." />
                <asp:RegularExpressionValidator ID="revPwd" runat="server"
                    ControlToValidate="txtPwd" Display="Dynamic" CssClass="vmsg"
                    ValidationExpression=".{6,}"
                    ErrorMessage="Password must be at least 6 characters." />
            </div>

            <div class="field">
                <label>Confirm password</label>
                <asp:TextBox ID="txtConfirm" runat="server" TextMode="Password" />
                <asp:RequiredFieldValidator ID="rfvConfirm" runat="server"
                    ControlToValidate="txtConfirm" Display="Dynamic" CssClass="vmsg"
                    ErrorMessage="Please confirm your password." />
                <asp:CompareValidator ID="cmpConfirm" runat="server"
                    ControlToValidate="txtConfirm" ControlToCompare="txtPwd"
                    Display="Dynamic" CssClass="vmsg"
                    ErrorMessage="Passwords do not match." />
            </div>

            <div class="field">
                <label>Username</label>
                <asp:TextBox ID="txtUsername" runat="server" />
                <asp:RequiredFieldValidator ID="rfvUsername" runat="server"
                    ControlToValidate="txtUsername" Display="Dynamic" CssClass="vmsg"
                    ErrorMessage="Username is required." />
            </div>

            <div class="field">
                <label>Date of birth</label>
                <asp:TextBox ID="txtDob" runat="server" TextMode="Date" />
                <asp:RequiredFieldValidator ID="rfvDob" runat="server"
                    ControlToValidate="txtDob" Display="Dynamic" CssClass="vmsg"
                    ErrorMessage="Date of birth is required." />
            </div>

            <div class="auth-row">
                <asp:Button ID="btnBack" runat="server" Text="Back" CssClass="btn-grey"
                    CausesValidation="false" OnClick="btnBack_Click" />
                <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn-primary"
                    OnClick="btnRegister_Click" />
            </div>
        </div>
    </form>
</body>
</html>
