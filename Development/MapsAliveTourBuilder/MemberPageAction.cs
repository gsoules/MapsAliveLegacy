// Copyright (C) 2003-2010 AvantLogic Corporation
public enum MemberPageActionId
{
	_NotSet,
	Account,
	AddDataSheet,
	AddHotspot,
	Announcements,
	AppContent,
	BannerOptions,
	BuildTour,
	BuyCustomServices,
	BuyHotspots,
	BuyPlan,
	BuyReceipt,
	LayoutAreaMarginsAndSpacing,
	CategoryExplorer,
	ChangeProfile,
	ColorSchemeExplorer,
	ConfigStatus,
	ControlPanel,
	CreateCategory,
	CreateColorScheme,
	CreateFontStyle,
	CreateMarker,
	CreateMarkerStyle,
	CreateTooltipStyle,
	CreateSymbol,
	CustomHtml,
	DeleteCategory,
	DeleteColorScheme,
	DeleteDataSheet,
	DeleteFontStyle,
	DeleteGallery,
	DeleteHotspot,
	DeleteHotspots,
	DeleteMap,
	DeleteMarker,
	DeleteMarkerStyle,
	DeleteStyle,
	DeleteSymbol,
	DeleteTooltipStyle,
	DeleteTour,
	DirectoryOptions,
	DisableMapZoom,
	DuplicateHotspot,
	DuplicateResource,
	DuplicateTour,
	EditCategory,
	EditColorScheme,
	EditFontStyle,
	EditHotspot,
	EditHotspotContent,
	EditHotspotActions,
	EditMarker,
	EditMarkerStyle,
	EditPage,
	EditSymbol,
	EditTooltipStyle,
	EnableMapZoom,
	Expired,
	Export,
	ExportArchive,
	ExportArchiveFullSize,
	ExportContentCsv,
	ExportContentXml,
	ExportImages,
	ExportPublishedTour,
	ExportResources,
	ExportResourcesAll,
	FontStyleExplorer,
	Gallery,
	GallerySetup,
	GallerySetupNew,
	HomePage,
	HotspotLimitReached,
	HotspotOptionsAdvanced,
	HotspotProperties,
	ImportAccounts,
	ImportArchive,
	ImportHotspotContent,
	ImportHotspotPhotos,
	ImportHotspots,
	ImportRoutes,
	ImportMarkerShapes,
	LayoutProperties,
	LayoutHelp,
	LastReport,
	LocateHotspot,
	Logout,
	ManageUsers,
	Map,
	MapMargins,
	MapOptionsAdvanced,
	MapProperties,
	MapSetup,
	MapSetupNew,
	MarkerExplorer,
	MarkerStyleExplorer,
	OrderHistory,
	PopupBehavior,
	PopupAppearance,
	Preferences,
	Profile,
	PublishTour,
	RecoverPassword,
	RenewMembership,
	ReplaceMarkers,
	ReplaceMarkerStyles,
	ReportAccountStatistics,
	Reports,
	ResourceHelp,
	Resources,
	Save,
	SessionExpired,
	SignUp,
	SymbolExplorer,
	TemplateChoicesForLayoutArea,
	TemplateChoicesForPopup,
	TemplateSplittersForLayoutArea,
	TemplateSplittersForPopup,
	TooltipStyleExplorer,
	TourBuilder,
	TourExplorer,
	TourLayoutAdvanced,
	TourManager,
	TourOptions,
	TourPreview,
	TourProperties,
	TourResourceDependencyReport,
	TourSetup,
	TourSetupNew,
	Undefined,
	Undo,
	UpgradeMembership,
	UploadMap,
	UserAccounts,
	Welcome
}

public enum PageUsage
{
	TourBuilder,
	Resources,
	Account
}

public enum ExportType
{
	Excel = 1,
	PublishedTour = 2
}

public class MemberPageAction
{
	public static string ActionPageTarget(MemberPageActionId actionId)
	{
		string targetPage = string.Empty;

		switch (actionId)
		{
			case MemberPageActionId.AddDataSheet:
				targetPage = "AddTourView.ashx?ds=1";
				break;

			case MemberPageActionId.AddHotspot:
				targetPage = "AddTourView.ashx";
				break;

			case MemberPageActionId.Announcements:
				targetPage = "Announcements.aspx";
				break;

			case MemberPageActionId.AppContent:
				targetPage = "../Admin/ContentManager.aspx";
				break;

			case MemberPageActionId.BannerOptions:
				targetPage = "BannerOptions.aspx";
				break;

			case MemberPageActionId.BuyCustomServices:
				targetPage = GetShoppingCartTargetPage(actionId);
				break;

			case MemberPageActionId.BuyHotspots:
				targetPage = GetShoppingCartTargetPage(actionId);
				break;

			case MemberPageActionId.BuyPlan:
				targetPage = GetShoppingCartTargetPage(actionId);
				break;

			case MemberPageActionId.BuyReceipt:
				targetPage = "BuyReceipt.aspx";
				break;

			case MemberPageActionId.LayoutAreaMarginsAndSpacing:
				targetPage = "CanvasMarginsAndSpacing.aspx";
				break;

			case MemberPageActionId.CategoryExplorer:
				targetPage = "CategoryExplorer.aspx";
				break;

			case MemberPageActionId.ChangeProfile:
				targetPage = "ChangeProfile.aspx";
				break;

			case MemberPageActionId.ColorSchemeExplorer:
				targetPage = string.Format("StyleExplorer.aspx?rt={0}", (int)TourResourceType.TourStyle);
				break;

			case MemberPageActionId.ConfigStatus:
				targetPage = "../Default.aspx?config=1";
				break;

			case MemberPageActionId.ControlPanel:
				targetPage = "../Admin/Admin.aspx";
				break;

			case MemberPageActionId.CreateCategory:
				targetPage = "EditCategory.aspx?id=new";
				break;

			case MemberPageActionId.CreateColorScheme:
				targetPage = string.Format("PerformAction.ashx?aid={0}&rt={1}", (int)actionId, (int)TourResourceType.TourStyle);
				break;

			case MemberPageActionId.CreateFontStyle:
				targetPage = string.Format("PerformAction.ashx?aid={0}&rt={1}", (int)actionId, (int)TourResourceType.FontStyle);
				break;

			case MemberPageActionId.CreateMarker:
				targetPage = string.Format("PerformAction.ashx?aid={0}&rt={1}", (int)actionId, (int)TourResourceType.Marker);
				break;

			case MemberPageActionId.CreateMarkerStyle:
				targetPage = string.Format("PerformAction.ashx?aid={0}&rt={1}", (int)actionId, (int)TourResourceType.MarkerStyle);
				break;

			case MemberPageActionId.CreateSymbol:
				targetPage = "EditSymbol.aspx?id=new";
				break;

			case MemberPageActionId.CreateTooltipStyle:
				targetPage = string.Format("PerformAction.ashx?aid={0}&rt={1}", (int)actionId, (int)TourResourceType.TooltipStyle);
				break;

			case MemberPageActionId.CustomHtml:
				targetPage = "CustomHtml.aspx";
				break;

			case MemberPageActionId.DeleteHotspots:
				targetPage = "DeleteHotspots.aspx";
				break;

			case MemberPageActionId.DeleteCategory:
			case MemberPageActionId.DeleteColorScheme:
			case MemberPageActionId.DeleteDataSheet:
			case MemberPageActionId.DeleteFontStyle:
			case MemberPageActionId.DeleteGallery:
			case MemberPageActionId.DeleteHotspot:
			case MemberPageActionId.DeleteMap:
			case MemberPageActionId.DeleteMarker:
			case MemberPageActionId.DeleteMarkerStyle:
			case MemberPageActionId.DeleteStyle:
			case MemberPageActionId.DeleteSymbol:
			case MemberPageActionId.DeleteTooltipStyle:
			case MemberPageActionId.DeleteTour:
				break;

			case MemberPageActionId.DirectoryOptions:
				targetPage = "DirectoryOptions.aspx";
				break;

			case MemberPageActionId.DuplicateHotspot:
				targetPage = "AddTourView.ashx?dup=1";
				break;

			case MemberPageActionId.DuplicateResource:
				targetPage = "PerformAction.ashx";
				break;

			case MemberPageActionId.DuplicateTour:
				targetPage = "DuplicateTour.aspx";
				break;

			case MemberPageActionId.EditCategory:
				targetPage = "EditCategory.aspx";
				break;

			case MemberPageActionId.EditColorScheme:
				targetPage = "EditColorScheme.aspx";
				break;

			case MemberPageActionId.EditFontStyle:
				targetPage = "EditFontStyle.aspx";
				break;

			case MemberPageActionId.EditHotspotActions:
				targetPage = "EditHotspotActions.aspx";
				break;

			case MemberPageActionId.EditHotspotContent:
				targetPage = "EditHotspotContent.aspx";
				break;

			case MemberPageActionId.EditMarker:
				targetPage = "EditMarker.aspx";
				break;

			case MemberPageActionId.EditMarkerStyle:
				targetPage = "EditMarkerStyle.aspx";
				break;

			case MemberPageActionId.EditPage:
				targetPage = "TourPageEditor.ashx";
				break;

			case MemberPageActionId.EditSymbol:
				targetPage = "EditSymbol.aspx";
				break;

			case MemberPageActionId.EditTooltipStyle:
				targetPage = "EditTooltipStyle.aspx";
				break;

			case MemberPageActionId.EditHotspot:
				targetPage = "TourViewEditor.ashx";
				break;

			case MemberPageActionId.Expired:
				targetPage = "Expired.aspx";
				break;

			case MemberPageActionId.Export:
				targetPage = "Export.aspx";
				break;
				
			case MemberPageActionId.ExportArchive:
			case MemberPageActionId.ExportArchiveFullSize:
			case MemberPageActionId.ExportContentCsv:
			case MemberPageActionId.ExportContentXml:
			case MemberPageActionId.ExportImages:
			case MemberPageActionId.ExportPublishedTour:
			case MemberPageActionId.ExportResources:
			case MemberPageActionId.ExportResourcesAll:
				break;

			case MemberPageActionId.FontStyleExplorer:
				targetPage = string.Format("StyleExplorer.aspx?rt={0}", (int)TourResourceType.FontStyle);
				break;

			case MemberPageActionId.Gallery:
				targetPage = "Gallery.aspx";
				break;

			case MemberPageActionId.GallerySetupNew:
				targetPage = "MapSetup.aspx?new=1&gallery=1";
				break;

			case MemberPageActionId.GallerySetup:
				targetPage = "MapSetup.aspx";
				break;

			case MemberPageActionId.HomePage:
				targetPage = "..";
				break;

			case MemberPageActionId.HotspotLimitReached:
				targetPage = "HotspotLimit.aspx";
				break;

			case MemberPageActionId.HotspotOptionsAdvanced:
				targetPage = "HotspotOptionsAdvanced.aspx";
				break;

			case MemberPageActionId.HotspotProperties:
				targetPage = "HotspotProperties.aspx";
				break;

			case MemberPageActionId.ImportAccounts:
				targetPage = "ImportAccounts.aspx";
				break;

			case MemberPageActionId.ImportArchive:
				targetPage = "ImportArchive.aspx";
				break;

			case MemberPageActionId.ImportHotspotContent:
				targetPage = "ImportHotspotContent.aspx";
				break;

			case MemberPageActionId.ImportHotspotPhotos:
				targetPage = "ImportHotspotPhotos.aspx";
				break;

			case MemberPageActionId.ImportHotspots:
				targetPage = "ImportHotspots.aspx";
				break;

			case MemberPageActionId.ImportRoutes:
				targetPage = "ImportRoutes.aspx";
				break;

			case MemberPageActionId.ImportMarkerShapes:
				targetPage = "ImportMarkerShapes.aspx";
				break;

			case MemberPageActionId.LayoutHelp:
				targetPage = "LayoutHelp.aspx";
				break;

			case MemberPageActionId.LastReport:
				targetPage = "LastReport.aspx";
				break;

			case MemberPageActionId.LayoutProperties:
				targetPage = "LayoutProperties.aspx";
				break;

			case MemberPageActionId.LocateHotspot:
				targetPage = "Map.aspx?locate=1";
				break;

			case MemberPageActionId.Logout:
				targetPage = "Logout.ashx";
				break;

			case MemberPageActionId.ManageUsers:
				targetPage = "ManageUsers.aspx";
				break;

			case MemberPageActionId.Map:
				targetPage = "Map.aspx";
				break;

			case MemberPageActionId.MapMargins:
				targetPage = "CanvasMarginsAndSpacing.aspx";
				break;

			case MemberPageActionId.MapOptionsAdvanced:
				targetPage = "MapOptionsAdvanced.aspx";
				break;

			case MemberPageActionId.MapProperties:
				targetPage = "MapProperties.aspx";
				break;

			case MemberPageActionId.MapSetup:
				targetPage = "MapSetup.aspx";
				break;

			case MemberPageActionId.MapSetupNew:
				targetPage = "MapSetup.aspx?new=1";
				break;

			case MemberPageActionId.MarkerExplorer:
				targetPage = "MarkerExplorer.aspx";
				break;

			case MemberPageActionId.MarkerStyleExplorer:
				targetPage = string.Format("StyleExplorer.aspx?rt={0}", (int)TourResourceType.MarkerStyle);
				break;

			case MemberPageActionId.OrderHistory:
				targetPage = "OrderHistory.aspx";
				break;

			case MemberPageActionId.PopupAppearance:
				targetPage = "PopupAppearance.aspx";
				break;
			
			case MemberPageActionId.PopupBehavior:
				targetPage = "PopupBehavior.aspx";
				break;

			case MemberPageActionId.Preferences:
				targetPage = "Preferences.aspx";
				break;

			case MemberPageActionId.Profile:
				targetPage = "Profile.aspx";
				break;
			
			case MemberPageActionId.RecoverPassword:
				targetPage = "../User/RecoverPassword.aspx";
				break;
			
			case MemberPageActionId.RenewMembership:
				targetPage = GetShoppingCartTargetPage(actionId);
				break;

			case MemberPageActionId.ReplaceMarkers:
				targetPage = "ReplaceMarkers.aspx";
				break;

			case MemberPageActionId.ReplaceMarkerStyles:
				targetPage = "ReplaceMarkerStyles.aspx";
				break;

			case MemberPageActionId.ReportAccountStatistics:
				targetPage = "../Admin/ReportAccountStatistics.aspx";
				break;

			case MemberPageActionId.Reports:
				targetPage = "../Admin/Reports.aspx";
				break;

			case MemberPageActionId.ResourceHelp:
				targetPage = "ResourceHelp.aspx";
				break;

			case MemberPageActionId.Resources:
				targetPage = "Resources.aspx";
				break;

			case MemberPageActionId.Save:
				break;

			case MemberPageActionId.SessionExpired:
				targetPage = "SessionExpired.aspx";
				break;

			case MemberPageActionId.SignUp:
				targetPage = "../User/SignUp.aspx";
				break;

			case MemberPageActionId.SymbolExplorer:
				targetPage = string.Format("StyleExplorer.aspx?rt={0}", (int)TourResourceType.Symbol);
				break;

			case MemberPageActionId.TemplateChoicesForLayoutArea:
			case MemberPageActionId.TemplateChoicesForPopup:
				targetPage = "TemplateChoices.aspx";
				break;

			case MemberPageActionId.TemplateSplittersForLayoutArea:
			case MemberPageActionId.TemplateSplittersForPopup:
				targetPage = "TemplateSplitters.aspx";
				break;

			case MemberPageActionId.TooltipStyleExplorer:
				targetPage = string.Format("StyleExplorer.aspx?rt={0}", (int)TourResourceType.TooltipStyle);
				break;

			case MemberPageActionId.TourBuilder:
				targetPage = string.Format("PerformAction.ashx?aid={0}", (int)MemberPageActionId.TourBuilder);
				break;

			case MemberPageActionId.TourExplorer:
				targetPage = "TourExplorer.aspx";
				break;

			case MemberPageActionId.TourLayoutAdvanced:
				targetPage = "TourLayoutAdvanced.aspx";
				break;

			case MemberPageActionId.TourManager:
				targetPage = "TourManager.aspx";
				break;

			case MemberPageActionId.TourOptions:
				targetPage = "TourOptions.aspx";
				break;

			case MemberPageActionId.TourPreview:
				targetPage = "TourPreview.aspx";
				break;

			case MemberPageActionId.TourProperties:
				targetPage = "TourProperties.aspx";
				break;

			case MemberPageActionId.TourResourceDependencyReport:
				targetPage = "TourResourceDependencyReport.aspx";
				break;

			case MemberPageActionId.TourSetup:
				targetPage = "TourSetup.aspx";
				break;

            case MemberPageActionId.TourSetupNew:
                targetPage = "TourSetup.aspx?new=1";
                break;

            case MemberPageActionId.Undo:
				break;

			case MemberPageActionId.UpgradeMembership:
				targetPage = GetShoppingCartTargetPage(actionId);
				break;

			case MemberPageActionId.UploadMap:
				targetPage = "UploadMap.aspx";
				break;

			case MemberPageActionId.UserAccounts:
				targetPage = "../Admin/Users.aspx";
				break;

			case MemberPageActionId.Welcome:
				targetPage = "Welcome.aspx";
				break;

			default:
				System.Diagnostics.Debug.Fail("Undefined ActionId " + actionId);
				break;
		}

		return targetPage;
	}

	private static string GetShoppingCartTargetPage(MemberPageActionId actionId)
	{
		OrderKind orderKind = OrderKind.NotSet;

		switch (actionId)
		{
			case MemberPageActionId.BuyHotspots:
				{
					orderKind = OrderKind.BuyHotspots;
					break;
				}
			
			case MemberPageActionId.BuyPlan:
				{
					orderKind = OrderKind.BuyPlan;
					break;
				}

			case MemberPageActionId.UpgradeMembership:
				{
					orderKind = OrderKind.UpgradePlan;
					break;
				}

			case MemberPageActionId.RenewMembership:
				{
					orderKind = OrderKind.RenewPlan;
					break;
				}

			case MemberPageActionId.BuyCustomServices:
				{
					orderKind = OrderKind.Payment;
					break;
				}
		}

        string pageName = "ShoppingCart";
        return string.Format("../Buy/{1}.aspx?order={0}", (int)orderKind, pageName);
	}

	public static string ActionPageTargetPath(MemberPageActionId actionId)
	{
		return "~/Members/" + MemberPageAction.ActionPageTarget(actionId);
	}

	public static bool IsDataSheetAction(MemberPageActionId actionId)
	{
		switch (actionId)
		{
			case MemberPageActionId.BannerOptions:
			case MemberPageActionId.LayoutAreaMarginsAndSpacing:
			case MemberPageActionId.TemplateChoicesForLayoutArea:
			case MemberPageActionId.TourLayoutAdvanced:
			case MemberPageActionId.LayoutHelp:
			case MemberPageActionId.TemplateSplittersForLayoutArea:
				return true;
		}
		return false;
	}

	public static bool IsPopupAction(MemberPageActionId actionId)
	{
		switch (actionId)
		{
			case MemberPageActionId.PopupAppearance:
			case MemberPageActionId.PopupBehavior:
				return true;
		}
		return false;
	}

	public static bool IsMapAction(MemberPageActionId actionId)
	{
		switch (actionId)
		{
			case MemberPageActionId.BannerOptions:
			case MemberPageActionId.LayoutAreaMarginsAndSpacing:
			case MemberPageActionId.Gallery:
			case MemberPageActionId.ImportRoutes:
			case MemberPageActionId.ImportHotspotPhotos:
			case MemberPageActionId.ImportMarkerShapes:
			case MemberPageActionId.LayoutHelp:
			case MemberPageActionId.Map:
			case MemberPageActionId.MapMargins:
			case MemberPageActionId.MapOptionsAdvanced:
			case MemberPageActionId.MapSetup:
			case MemberPageActionId.MapSetupNew:
			case MemberPageActionId.PopupAppearance:
			case MemberPageActionId.PopupBehavior:
			case MemberPageActionId.TemplateChoicesForLayoutArea:
			case MemberPageActionId.TemplateChoicesForPopup:
			case MemberPageActionId.TemplateSplittersForLayoutArea:
			case MemberPageActionId.TemplateSplittersForPopup:
			case MemberPageActionId.TourLayoutAdvanced:
			case MemberPageActionId.UploadMap:
				return true;
		}
		return false;
	}

	public static bool IsHotspotAction(MemberPageActionId actionId)
	{
		switch (actionId)
		{
			case MemberPageActionId.EditHotspotActions:
			case MemberPageActionId.EditHotspotContent:
			case MemberPageActionId.HotspotOptionsAdvanced:
			case MemberPageActionId.HotspotLimitReached:
				return true;
		}
		return false;
	}
}
