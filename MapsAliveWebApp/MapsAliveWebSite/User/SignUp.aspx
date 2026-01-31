<%@
	Page Language="C#"
	MasterPageFile="~/Masters/Secure.master"
	AutoEventWireup="true"
	CodeFile="SignUp.aspx.cs"
	Inherits="User_SignUp"
	Title="MapsAlive SignUp"
%>

<%@ Register TagName="SignUp" TagPrefix="uc" Src="~\Controls\SignUp.ascx" %>

<asp:Content ContentPlaceHolderID="ContentBody" Runat="Server">
    <script src="https://www.google.com/recaptcha/api.js" async defer></script>
	<script type="text/javascript">
	var allowEnterKeyToPost = true;
	function maOnSignUp(token)
	{
        var e = document.getElementById('<%= StatusLine.ClientID %>');
		e.innerHTML = "Please wait while we set up your account...";
		e = document.getElementById('menu');
        e.style.visibility = "hidden";
        __doPostBack('OnSignUp', '');
	}
    </script>
		
	<div class="securePageContent">
		<div class="pageDivider"></div>
		<div class="pageTitle">Create a MapsAlive Account</div>
		<div class="pageDivider"></div>

		<div style="display:flex;justify-content: space-around;">
            <div id="menu" style="display:flex; width:150px;justify-content:space-evenly;">
			    <div><asp:LinkButton runat="server" PostBackUrl="~/User/Signup.aspx?login=0" OnClick="OnGoHome">Home </asp:LinkButton></div>
			    <div><asp:LinkButton runat="server" PostBackUrl="~/User/Signup.aspx?login=0" OnClick="OnPlans">Pricing</asp:LinkButton></div>
		    </div>
        </div>
						
		<div class="formTable" style="margin-top:24px;">
			<div>
                <asp:Panel runat="server" ID="PageHeading" class="secureSubHeading">
                    Sign Up for a 30 Day Free Trial:
                </asp:Panel>
                <asp:Panel runat="server" class="subHead" style="margin-bottom:24px;">
                    <ul class="textNormal" style="margin-left: 12px;font-weight:normal;">
                        <li style="margin-bottom: 4px;"><span style="">Create interactive maps and galleries</span></li>
                        <li style="margin-bottom: 4px;"><span style="">Use up to 300 hotspots</span></li>
                        <li style="margin-bottom: 4px;"><span style="">Try all the features of the Pro Plan*</li>
                    </ul>
                </asp:Panel>
                        
                <asp:Panel runat="server" class="subHead" style="margin-bottom:12px;font-size:14px;font-style:italic;">
                    No credit card information is required to sign up
                </asp:Panel>
            </div>
			
			<uc:SignUp Id="SignUpControl" runat="server" />
			
			<div class="textNormal formTable">
				<div>
					<div class="formMainBoxGreen" style="text-align:center;padding-bottom:4px;">
						<button class="formMainSignUpButton" onclick="maOnSignUp()">Submit>
                            <img src="../Images/BtnSignUp.gif" />
						</button>
					</div>
					
					<div style="margin-top:12px;">
						<div style="color:#cf7005;margin-top:4px;margin-bottom:0px;font-size:16px;font-weight:bold;">
							<asp:Label ID="StatusLine" runat="server" Text="&nbsp;"/>
						</div>
					</div>
					
					<asp:Panel ID="PrivacyPanel" runat="server">
						<div class="textSmall" style="margin-top:4px;font-size:14px;">
							<div style="margin-top:10px;">* The free trial includes all Pro Plan features except Download and Archive. Maps created with the trial display <span style="font-style:italic;">Created with MapsAlive Trial</span>. The message goes away after you purchase a plan.</div>
							<div class="subHead" style="margin-top:12px;">Your Privacy</div>
							<div>We will not share your account information with anyone.</div>
						</div>
					</asp:Panel>
					
				</div>
			</div>
		</div>
	</div>
</asp:Content>

