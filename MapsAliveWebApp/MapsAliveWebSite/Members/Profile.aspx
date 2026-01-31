<%@
	Page Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="Profile.aspx.cs"
	Inherits="Members_Profile"
	Title="MapsAlive Account"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal">
		<asp:Panel ID="StopImpersonationPanel" runat="server" Visible="false" style="margin-top:0px;margin-bottom:8px;">
			<asp:LinkButton ID="StopImpersonationButton" runat="server" OnClick="OnStopImpersonation" />
		</asp:Panel>
	  
	   	<div class="optionsSectionTitleFirst">Account Profile</div>
	   	<table cellspacing="6">
			<tr>
				<td class="profileLabel">Account #:</td>
				<td><asp:Label ID="AccountNumber" runat="server" /></td>
			</tr>
	   		<tr>
	   			<td class="profileLabel">Plan:</td>
	   			<td><asp:Label ID="AccountType" runat="server" /></td>
	   		</tr>
	   		<tr ID="DaysRemainingRow" runat="server">
	   			<td class="profileLabel">Days Remaining:</td>
	   			<td><asp:Label ID="DaysRemaining" runat="server"></asp:Label><asp:Label ID="ExpiryDate" runat="server" /></td>
	   		</tr>
	   		<tr id="CreditBalanceRow" runat="server">
	   			<td class="profileLabel">Credit Balance:</td>
	   			<td><asp:Label ID="CreditBalance" runat="server"/></td>
	   		</tr>
	   		<tr>
	   			<td class="profileLabel">Tours:</td>
	   			<td><asp:Label ID="Tours" runat="server"/></td>
	   		</tr>
	   		<tr ID="HotspotsAllowedRow" runat="server">
	   			<td class="profileLabel">Hotspots Owned:</td>
	   			<td><asp:Label ID="HotspotLimit" runat="server"/></td>
	   		</tr>
	   		<tr ID="HotspotsUsedRow" runat="server">
	   			<td class="profileLabel">Hotspots Used:</td>
	   			<td><asp:Label ID="SlidesUsed" runat="server"/></td>
	   		</tr>
	   		<tr ID="HotspotsBorrowedRow" runat="server">
	   			<td class="profileLabel">Hotspots Borrowed:</td>
	   			<td><asp:Label ID="SlidesBorrowed" runat="server"/></td>
	   		</tr>
	   		<tr ID="HotspotsAvailableRow" runat="server">
	   			<td class="profileLabel">Hotspots Available:</td>
	   			<td><asp:Label ID="SlidesAvailable" runat="server"/></td>
	   		</tr>
	   	</table>
	   	
		<asp:Panel ID="AdminPanel" runat="server" Visible="false" style="padding:8px;margin-top:8px;background-color:#e7f0f6;">
			<table>
				<tr>
					<td valign="top">
						<table>
							<tr>
								<td align="right" style="padding-bottom:8px;"><b>Password:</b></td>
								<td style="padding-bottom:8px;"><asp:Label ID="LabelUserPw" runat="server" /></td>
							</tr>
							<tr ID="TypeRow" runat="server">
								<td align="right"><b>Type:</b></td>
								<td>
									<asp:DropDownList ID="AccountTypeList" runat="server">
										<asp:ListItem Text="Trial" Value="6" />
										<asp:ListItem Text="Paid" Value="7" />
										<asp:ListItem Text="Elite" Value="5" />
									</asp:DropDownList>
								</td>
							</tr>
							<tr>
								<td align="right"><b>Plan:</b></td>
								<td>
									<asp:DropDownList ID="AccountPlanList" runat="server">
										<asp:ListItem Text="Starter" Value="4" />
										<asp:ListItem Text="Basic" Value="1" />
										<asp:ListItem Text="Plus" Value="2" />
										<asp:ListItem Text="Pro" Value="3" />
									</asp:DropDownList>
								</td>
							</tr>
							<tr ID="DaysRow" runat="server">
								<td align="right"><b>Days:</b></td>
								<td>
									<asp:TextBox ID="DaysTextBox" runat="server" Width="40px" />
									<asp:Label ID="DaysError" runat="server" CssClass="textErrorMessage" />
								</td>
							</tr>
							<tr ID="Hotspots" runat="server">
								<td align="right"><b>Hotspots:</b></td>
								<td>
									<asp:TextBox ID="HotspotsTextBox" runat="server" Width="40px" />
									<asp:Label ID="HotspotsError" runat="server" CssClass="textErrorMessage" />
								</td>
							</tr>
							<tr ID="CreditRow" runat="server">
								<td align="right"><b>Credit:</b></td>
								<td>
									<asp:TextBox ID="CreditTextBox" runat="server" Width="60px" />
									<asp:Label ID="CreditError" runat="server" CssClass="textErrorMessage" />
								</td>
							</tr>
							<tr ID="PaymentRow" runat="server">
								<td align="right"><b>Payment:</b></td>
								<td>
									<asp:TextBox ID="PaymentTextBox" runat="server" Width="60px" />
									<asp:Label ID="PaymentError" runat="server" CssClass="textErrorMessage" />
								</td>
							</tr>
							<tr ID="DiscountRow" runat="server">
								<td align="right"><b>Discount:</b></td>
								<td>
									<asp:TextBox ID="DiscountTextBox" runat="server" Width="20px" />%
									<asp:Label ID="DiscountError" runat="server" CssClass="textErrorMessage" />
								</td>
							</tr>
						</table>
					</td>
					<td valign="top">
						<table>
							<tr ID="ReportsRow" runat="server">
								<td width="100"></td>
								<td style="padding-bottom:16px;font-weight:bold;">
									<a class="pageActionControl" onclick="maDoPostBack('OnViewReports', '');">View Reports</a>
								</td>
							</tr>
							<tr>
								<td></td>
								<td style="padding-bottom:16px;font-weight:bold;">
									<a class="pageActionControl" onclick="maDoPostBack('OnReturnToUsers', '');">Return to Users</a>
								</td>
							</tr>
							<tr ID="ActivateToursRow" runat="server">
								<td></td>
								<td>
									<a class="pageActionControl" onclick="maConfirmAndPostBack('Activate Tours?','OnActivateTours','ACTIVATE');">Activate Tours</a>
								</td>
							</tr>
							<tr ID="DeactivateToursRow" runat="server">
								<td></td>
								<td>
									<a class="pageActionControl" onclick="maConfirmAndPostBack('Deactivate Tours?','OnDeactivateTours','DEACTIVATE');">Deactivate Tours</a>
								</td>
							</tr>
							<tr ID="PurgeAccountRow" runat="server">
								<td></td>
								<td>
									<a class="pageActionControl" onclick="maConfirmAndPostBack('Delete all tours from this account?','OnPurgeAccount','DELETE TOURS');">Delete all Tours from this Account</a>
								</td>
							</tr>
							<tr>
								<td></td>
								<td>
									<a class="pageActionControl" onclick="maConfirmAndPostBack('Delete this account?','OnDeleteAccount','DELETE ACCOUNT');">Delete Account</a>
                                    <asp:Label ID="AccountOrders" runat="server" style="font-weight:bold;color:firebrick;"></asp:Label>
								</td>
							</tr>
						</table>
					</td>
				</tr>
			</table>
			<div style="text-align:center;">
				<asp:Label ID="SessionTextBox" runat="server" />
			</div>
		</asp:Panel>
		    
	   	<div class="optionsSectionTitle">User Profile</div>

		<table>
			<tr>
				<td class="fieldLabel"><asp:Label runat="server" Text="Contact Name:" /></td>
				<td><asp:TextBox ID="ContactNameTextBox" runat="server"  CssClass="fieldTextBox250" /></td>
			</tr>
			<tr>
				<td class="fieldLabel"><asp:Label runat="server" Text="Email: " /></td>
				<td><asp:TextBox ID="EmailTextBox" runat="server"  CssClass="fieldTextBox250" /></td>
			</tr>
			<tr ID="NewsletterRow" runat="server">
				<td/>
				<td>
					<asp:CheckBox ID="NewsletterCheckbox" runat="server" />
					<asp:Label ID="Label3" runat="server" Text="<%$ Resources:Text, NewsletterOptIn %>" />
				</td>
			</tr>
			<tr>
				<td></td><td style="padding-top:12px;font-weight:bold;">Change Password</td>
			</tr>
			<tr>
				<td class="fieldLabel"><asp:Label runat="server" Text="Old Password: " /></td>
				<td><asp:TextBox ID="OldPasswordTextBox" runat="server" autocomplete="off" TextMode="Password" /></td>
			</tr>
			<tr>
				<td class="fieldLabel"><asp:Label runat="server" Text="New Password: " /></td>
				<td><asp:TextBox ID="NewPasswordTextBox" runat="server" autocomplete="off" TextMode="Password" /></td>
			</tr>
			<tr>
				<td class="fieldLabel"><asp:Label runat="server" Text="Confirm Password: " /></td>
				<td><asp:TextBox ID="ConfirmPasswordTextBox" runat="server" autocomplete="off" TextMode="Password" /></td>
			</tr>
			<tr>
				<td>&nbsp;</td><td></td>
			</tr>
		</table>
		
	</div>
</asp:Content>

