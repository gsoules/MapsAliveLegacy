// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Web.UI.WebControls;
using System.Data;

public partial class Members_EditMarkerStyle : EditStylePage
{
	private string validNormalFillColor;
	private string validNormalLineColor;
	private string validSelectedFillColor;
	private string validSelectedLineColor;
	
	protected override void EmitJavaScript()
	{
		string loadingScript = string.Empty;
				
		string loadedScript = string.Format("maInitMarkerStylePreview({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}');",
			markerStyle.LineWidth,
			markerStyle.NormalFillColor,
			markerStyle.NormalFillColorOpacity,
			markerStyle.NormalLineColor,
			markerStyle.NormalLineColorOpacity,
			markerStyle.SelectedFillColor,
			markerStyle.SelectedFillColorOpacity,
			markerStyle.SelectedLineColor,
			markerStyle.SelectedLineColorOpacity
			);
		
		EmitJavaScript(loadingScript, loadedScript);
	}

	protected override void InitControls(bool undo)
	{
        StyleId.Text = markerStyle.Id.ToString();

        MemberPage.InitShowUsageControl(ShowUsageControl, markerStyle);

		LineWidthValue.Text = markerStyle.LineWidth.ToString() + " px";

		NormalFillColorOpacityValue.Text = markerStyle.NormalFillColorOpacity.ToString() + "%"; ;

		NormalLineColorOpacityValue.Text = markerStyle.NormalLineColorOpacity.ToString() + "%"; ;

		SelectedFillColorOpacityValue.Text = markerStyle.SelectedFillColorOpacity.ToString() + "%"; ;

		SelectedLineColorOpacityValue.Text = markerStyle.SelectedLineColorOpacity.ToString() + "%"; ;

		NormalEffectsTextBox.Text = markerStyle.NormalShapeEffects;
		AddChangeDetection(NormalEffectsTextBox);

		SelectedEffectsTextBox.Text = markerStyle.SelectedShapeEffects;
		AddChangeDetection(SelectedEffectsTextBox);

		if (!undo && IsPostBack)
			return;

		MarkerStyleNameTextBox.Text = markerStyle.Name;
		AddChangeDetection(MarkerStyleNameTextBox);

		NormalFillColorSwatch.ColorValue = markerStyle.NormalFillColor;
		NormalLineColorSwatch.ColorValue = markerStyle.NormalLineColor;
		SelectedFillColorSwatch.ColorValue = markerStyle.SelectedFillColor;
		SelectedLineColorSwatch.ColorValue = markerStyle.SelectedLineColor;
		
		SliderLineWidth.Value = markerStyle.LineWidth;
		SliderNormalFillColorOpacity.Value = markerStyle.NormalFillColorOpacity;
		SliderNormalLineColorOpacity.Value = markerStyle.NormalLineColorOpacity;
		SliderSelectedFillColorOpacity.Value = markerStyle.SelectedFillColorOpacity;
		SliderSelectedLineColorOpacity.Value = markerStyle.SelectedLineColorOpacity;

        NewMarkerStyleControl.OnClickActionId = MemberPageActionId.CreateMarkerStyle;
        NewMarkerStyleControl.Title = "Duplicate this marker style";
        NewMarkerStyleControl.QueryString = string.Format("&id={0}", markerStyle.Id);
        NewMarkerStyleControl.WarningMessage = string.Format("Click OK to create a duplicate of [@{0}@]. You will then be taken to the Edit Marker Style screen so that you can edit your new marker style.", markerStyle.Name);
    }

    protected override void PageLoad()
	{
		Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);
		SetPageTitle(TourResource.GetTitleForEditPage(TourResourceType.MarkerStyle));
		SetActionId(MemberPageActionId.EditMarkerStyle);
		GetSelectedTourOrNone();

		markerStyle = (MarkerStyle)CreateResourceFromQueryStringId(TourResourceType.MarkerStyle);

		if (markerStyle == null)
		{
			// This can happen if a user deletes a marker style and then uses the Back button to return to this screen.
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.MarkerStyleExplorer));
		}
	}

    protected override void PerformUpdate()
    {
        if (tour != null)
        {
            foreach (TourPage tourPage in tour.TourPages)
                tourPage.MapMarkerChanged();
        }
        
        base.PerformUpdate();
    }

    protected override void ReadPageFields()
	{
		if (markerStyle.Name != validName)
			markerStyle.Name = validName;

		markerStyle.LineWidth = (int)SliderLineWidth.Value;

		markerStyle.NormalFillColor = validNormalFillColor;
		markerStyle.NormalFillColorOpacity = (int)SliderNormalFillColorOpacity.Value;

		markerStyle.NormalLineColor = validNormalLineColor;
		markerStyle.NormalLineColorOpacity = (int)SliderNormalLineColorOpacity.Value;

		markerStyle.SelectedFillColor = validSelectedFillColor;
		markerStyle.SelectedFillColorOpacity = (int)SliderSelectedFillColorOpacity.Value;

		markerStyle.SelectedLineColor = validSelectedLineColor;
		markerStyle.SelectedLineColorOpacity = (int)SliderSelectedLineColorOpacity.Value;

		markerStyle.NormalShapeEffects = NormalEffectsTextBox.Text;
		markerStyle.SelectedShapeEffects = SelectedEffectsTextBox.Text;
	}

	protected override void ValidatePage()
	{
		ValidateResourceName(MarkerStyleNameTextBox, MarkerStyleNameError);
		if (!fieldValid)
			return;

		validNormalFillColor = ValidateColorSwatch(NormalFillColorSwatch);
		validNormalLineColor = ValidateColorSwatch(NormalLineColorSwatch);
		validSelectedFillColor = ValidateColorSwatch(SelectedFillColorSwatch);
		validSelectedLineColor = ValidateColorSwatch(SelectedLineColorSwatch);
		
		if (!fieldValid)
			return;
	}

	protected override void ClearErrors()
	{
		ClearErrors(MarkerStyleNameError);
	}
}
