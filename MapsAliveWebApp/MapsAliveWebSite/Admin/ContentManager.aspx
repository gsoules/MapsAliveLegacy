<%@
	Page Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="ContentManager.aspx.cs"
	Inherits="Admin_ContentManager"
	Title="MapsAlive Content Manager"
	ValidateRequest="false"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script language="javascript">   
        tinymce.init({
            selector: `#${formContentId}HtmlEditor`,
            content_css: '../Styles/Admin.css',
            content_style: 'body { margin:8px 4px;}',
            min_height: 200,
            max_width: 1600,
            resize: 'both',
            removed_menuitems: 'newdocument visualaid',
            plugins: 'code paste autoresize link lists charmap media image charmap hr help searchreplace table',
            paste_enable_default_filters: true,
            forced_root_block: false,
            fontsize_formats: "8px 10px 12px 14px 18px 24px 36px",
            branding: false,
            autoresize_bottom_margin: 0,
            menubar: 'format custom',
            toolbar: 'searchreplace undo redo | bold italic underline | charmap hr | bullist numlist table | link unlink | image media | help code',
            style_formats: [
                { title: 'Features Subhead', inline: 'span', classes: 'featuresSubhead' },
                { title: 'Menu Reference', inline: 'span', classes: 'menuReference' },
                { title: 'Plus Plan', inline: 'span', classes: 'planPlus' },
                { title: 'Pro Plan', inline: 'span', classes: 'planPro' },
                { title: 'FAQ Steps Number', selector: 'ol', classes: 'faqSteps' },
                { title: 'FAQ Steps Bullet', selector: 'ul', classes: 'faqSteps' },
                { title: 'Bullet List', selector: 'ul', classes: 'bullets' }
            ],
            menu: {
                custom: { title: 'Snippets', items: 'classicVsFlex faqBulletList faqNumberList proPlan plusAndProPlans featureInPlusAndPro featureInPro userGuideHotspots userGuideApi' }
            },
            setup: function (editor)
            {
                editor.on('Change', function (e)
                {
                    maChangeDetected();
                });

                editor.ui.registry.addMenuItem('classicVsFlex', {
                    text: 'Classic vs Flex',
                    onAction: function ()
                    {
                        editor.insertContent(`
                            <span class="featuresSubhead">Classic Tour</span>
                            <br /><br />
                            <span class="featuresSubhead">Flex Map</span><br /><br />`);
                    }
                });

                editor.ui.registry.addMenuItem('faqBulletList', {
                    text: 'FAQ Bullet List',
                    onAction: function ()
                    {
                        editor.insertContent(`
                            <ul class=faqSteps>
					        <li>Insert list item here.</li>
					        <li>Insert list item here.</li>
				            </ul>`);
                    }
                });

                editor.ui.registry.addMenuItem('faqNumberList', {
                    text: 'FAQ Number List',
                    onAction: function ()
                    {
                        editor.insertContent(`
                            <ol class=faqSteps>
					        <li>Insert list item here.</li>
					        <li>Insert list item here.</li>
				            </ol>`);
                    }
                });

                editor.ui.registry.addMenuItem('proPlan', {
                    text: 'Pro Plan',
                    onAction: function ()
                    {
                        editor.insertContent('<span class="planPro">(Pro Plan)</span>');
                    }
                });

                editor.ui.registry.addMenuItem('plusAndProPlans', {
                    text: 'Plus and Pro Plans',
                    onAction: function ()
                    {
                        editor.insertContent('<span class="planPlus">(Plus and Pro Plans)</span>');
                    }
                });

                editor.ui.registry.addMenuItem('featureInPlusAndPro', {
                    text: 'Feature in Plus and Pro Plans',
                    onAction: function ()
                    {
                        editor.insertContent('<strong>Note:</strong> The FEATURE is available in the <span class="planPlus">Plus and Pro Plans</span>');
                    }
                });

                editor.ui.registry.addMenuItem('featureInPro', {
                    text: 'Feature in Pro Plan',
                    onAction: function ()
                    {
                        editor.insertContent('<strong>Note:</strong> The FEATURE is available in the <span class="planPlus">Pro Plan</span>');
                    }
                });

                editor.ui.registry.addMenuItem('userGuideHotspots', {
                    text: 'User Guide Hotspots and Markers',
                    onAction: function ()
                    {
                        editor.insertContent('For complete documentation on Hotspots and Markers, choose <span class="menuReference">Help &gt; Learning Center</span> and see the <em>MapsAlive User Guide on Hotspots and Markers</em>.');
                    }
                });

                editor.ui.registry.addMenuItem('userGuideApi', {
                    text: 'User Guide API',
                    onAction: function ()
                    {
                        editor.insertContent('For complete documentation on the JavaScript API, choose <span class="menuReference">Help &gt; Learning Center</span> and see the <em>MapsAlive User Guide to the JavaScript API</em>.');
                    }
                });
            }
        });

        function okToDeleteTopic()
        {
            maConfirmAndPostBack("Press DELETE to remove the topic from the database.", "EventOnDelete", "DELETE")
            return false;
        }
    </script>
	<div class="textNormal" style="margin-top:6px;">
		Search for
		<asp:TextBox ID="Filter" runat="server" style="font-size:11px;"/>
		in
		<asp:DropDownList ID="FilterType" runat="server" style="font-size:11px;">
			<asp:ListItem Text="Topic"/>
			<asp:ListItem Text="Text"/>
			<asp:ListItem Text="All"/>
		</asp:DropDownList>
		<asp:Button ID="Button1" runat="server" Text="Search" OnClick="OnFilter" style="height:20px;font-size:10px;" />
		<asp:Button ID="Button2" runat="server" Text="Clear" OnClick="OnFilterClear" style="height:20px;font-size:10px;" />
		&nbsp;&nbsp;
		<b><asp:Label ID="Results" runat="server"/></b>
	</div>

	<div style="height:254px;margin-top:12px;overflow-y:scroll;overflow-x:hidden;border-bottom:solid 2px #777;">
		<asp:GridView
			ID="GridView"
			runat="server"
			OnSelectedIndexChanged="OnSelectRow"
			CssClass="textNormal"
			AutoGenerateColumns="False"
			AllowSorting="False"
			CellPadding="4"
			AllowPaging="False"
			Width="716px"
			>
			<Columns>
				<asp:CommandField ButtonType="Button" SelectText="Edit" ShowSelectButton="True" ItemStyle-Width="30px">
					<ControlStyle CssClass="buttonGridView" />
				</asp:CommandField>
				<asp:BoundField DataField="Id" HeaderText="Id" InsertVisible="False" ReadOnly="True" SortExpression="Id" />
				<asp:BoundField DataField="Topic" HeaderText="Topic" SortExpression="Topic" />
			</Columns>
			<RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
			<EditRowStyle BackColor="#999999" />
			<SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
			<HeaderStyle BackColor="#777777" Font-Bold="True" ForeColor="White" />
			<AlternatingRowStyle BackColor="White" ForeColor="#284775" />
		</asp:GridView>
	</div>
	
	<asp:Panel ID="EditorControlsPanel" runat="server" style="margin-top:16px; margin-bottom:4px;" CssClass="textNormal" Visible="false">
		Topic: <asp:TextBox ID="TopicName" runat="server" CssClass="fieldTextBox250"/>
		<asp:Button runat="server" Text="Delete" OnClientClick="return okToDeleteTopic()" />
	    <asp:Button runat="server" ID="SaveButton" Text="Save" OnClick="OnSave" />
	    <asp:Button runat="server" ID="CancelButton" Text="Cancel" OnClick="OnCancel" />
	</asp:Panel>

	<asp:Panel ID="HtmlEditorPanel" runat="server" style="margin:16px 0;" Visible="false">
        <asp:TextBox ID="HtmlEditor" runat="server" />
	</asp:Panel>

	<hr />
	<div class="textNormal" style="margin-top:12px;">
		<span class="fieldLabel">New Topic:</span> 
		<asp:TextBox ID="NewTopic" runat="server" CssClass="fieldTextBox250"/>
		<asp:Button runat="server" Text="Add" OnClick="OnAdd" />
		<br />
		<b><asp:Label CssClass="textErrorMessage" ID="NewTopicMessage" runat="server"/></b>
	</div>
	
	<div style="margin-top:24px;font-size:10px;color:#555;">
		Flush the App Content cache<br /><br />
		<asp:Button runat="server" style="font-size:10px;" Text="Flush Cache" OnClick="OnFlushCache" />
	</div>
	
	<asp:HiddenField ID="TopicId" runat="server" />
</asp:Content>

