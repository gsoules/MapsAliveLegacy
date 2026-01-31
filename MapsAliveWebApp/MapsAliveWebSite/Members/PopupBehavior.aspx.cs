// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Members_PopupBehavior : MemberPage
{
	private int validMarkerOffset;
	private bool popupOptionChanged;
	private int validPopupDelay;
	private int validPopupSlideX;
	private int validPopupSlideY;
	
	protected override void EmitJavaScript()
	{
		string loadingScript = string.Empty;
		string loadedScript = string.Format("maLocationChanged(true,{0});", (int)tourPage.PopupOptions.Location);
		EmitJavaScript(loadingScript, loadedScript);
	}

	protected void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	protected override void InitControls(bool undo)
	{
		if (!undo && IsPostBack)
			return;

		SetSelectedPopupLocation(tourPage.PopupOptions.Location);

		BestSideSequenceDropDown.SelectedValue = ((int)tourPage.PopupOptions.BestSideSequence).ToString();
		AddChangeDetection(BestSideSequenceDropDown);

		DelayDropDown.SelectedValue = ((int)tourPage.PopupOptions.DelayType).ToString();
		AddChangeDetection(DelayDropDown);

		PopupDelay.Text = tourPage.PopupOptions.Delay.ToString();
		AddChangeDetection(PopupDelay);

		PinOnClickCheckBox.Checked = tourPage.PopupOptions.PinOnClick;
		AddChangeDetection(PinOnClickCheckBox, "maOptionsChanged();");

		ShowTooltipCheckBox.Checked = tourPage.PopupOptions.ShowTooltipWhenNoContent;
		AddChangeDetection(ShowTooltipCheckBox);
		
		PopupMarkerOffset.Text = tourPage.PopupOptions.MarkerOffset.ToString();
		AddChangeDetection(PopupMarkerOffset);
	
		PopupSlideX.Text = tourPage.PopupOptions.LocationPoint.X.ToString();
		AddChangeDetection(PopupSlideX);

		PopupSlideY.Text = tourPage.PopupOptions.LocationPoint.Y.ToString();
		AddChangeDetection(PopupSlideY);

		HelpPanelText.Text = AppContent.Topic("HelpPopupLocationFixedAlwaysVisible");

		if (tour.V3CompatibilityEnabled)
        {
            PinMessage.Text = tourPage.PopupOptions.PinMessage;
		    AddChangeDetection(PinMessage);
        }
        else
        {
            PinMessagePanel.Visible = false;
            RadioButton6.Visible = false;
            RadioButton7.Visible = false;
        }

        // These values are known in mapsalive-popup.js. If you change them here, also change them there.
        PopupArrowType arrowType = tourPage.PopupOptions.ArrowType;
        AddArrowListItem(0, PopupArrowType.None);
        AddArrowListItem(20, PopupArrowType.Small);
        AddArrowListItem(32, PopupArrowType.Large);
        if (account.IsPlusOrProPlan && tour.V4)
        {
            AddArrowListItem((int)PopupArrowType.Callout50, PopupArrowType.Callout50);
            AddArrowListItem((int)PopupArrowType.Callout75, PopupArrowType.Callout75);
            AddArrowListItem((int)PopupArrowType.Callout100, PopupArrowType.Callout100);
            AddArrowListItem((int)PopupArrowType.Callout125, PopupArrowType.Callout125);
            AddArrowListItem((int)PopupArrowType.Callout150, PopupArrowType.Callout150);
            AddArrowListItem((int)PopupArrowType.Callout175, PopupArrowType.Callout175);
            AddArrowListItem((int)PopupArrowType.Callout200, PopupArrowType.Callout200);
            AddArrowListItem((int)PopupArrowType.Callout225, PopupArrowType.Callout225);
            AddArrowListItem((int)PopupArrowType.Callout250, PopupArrowType.Callout250);
            AddArrowListItem((int)PopupArrowType.Callout275, PopupArrowType.Callout275);
            AddArrowListItem((int)PopupArrowType.Callout300, PopupArrowType.Callout300);
        }
        else
        {
            // Convert a callout to the large arrow type. This is necessary when someone was using
            // a callout with a Plus Plan or higher, but now they are using a Personal Plan.
            if (arrowType != PopupArrowType.None && arrowType != PopupArrowType.Small && arrowType != PopupArrowType.Large)
                arrowType = PopupArrowType.Large;
        }

		ArrowDropDown.SelectedValue = ((int)arrowType).ToString();
		AddChangeDetection(ArrowDropDown);

        if (tour.V3CompatibilityEnabled)
            PopupsShowArrow.Visible = true;
        else
            PopupsCallout.Visible = true;

	}
    private void AddArrowListItem(int size, PopupArrowType arrowType)
    {
        string text;
        if (arrowType == PopupArrowType.None)
            text = "None";
        else
        {
            if (tour.V3CompatibilityEnabled)
            {
                if (size == 20)
                    text = "Small";
                else
                    text = "Large";
            }
            else
            {
                text = string.Format("{0}px", size);
            }
        }
        int id = (int)arrowType;
        ListItem item = new ListItem(text, id.ToString());
        ArrowDropDown.Items.Add(item);
    }

    protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Popup Behavior");
		SetActionIdForPageAction(MemberPageActionId.PopupBehavior);
		GetSelectedTourPage();
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
	
	protected override void PerformUpdate()
	{
		if (popupOptionChanged)
		{
			tourPage.RebuildMap();
			tourPage.SetLayoutChanged();
		}

		tourPage.UpdateDatabase();
	}
		
	protected override void ReadPageFields()
	{
		PopupOptions popupOptions = tourPage.PopupOptions;

		PopupLocation newPopupLocation = GetSelectedPopupLocation();

		if (popupOptions.Location != newPopupLocation)
		{
			popupOptions.Location = newPopupLocation;
			popupOptionChanged = true;
		}

		if (popupOptions.LocationIsFixed)
		{
			Point newLocation = new Point(validPopupSlideX, validPopupSlideY);
			if (popupOptions.LocationPoint != newLocation)
			{
				popupOptions.LocationPoint = new Point(validPopupSlideX, validPopupSlideY);
				popupOptionChanged = true;
			}
		}
		else
		{
			if (popupOptions.MarkerOffset != validMarkerOffset)
			{
				popupOptions.MarkerOffset = validMarkerOffset;
				popupOptionChanged = true;
			}

		    PopupArrowType arrowType = (PopupArrowType)int.Parse(ArrowDropDown.SelectedValue);
		    if (popupOptions.ArrowType != arrowType)
            {
			    popupOptions.ArrowType = arrowType;
                popupOptionChanged = true;
            }
        }

		if (tourPage.PopupOptions.Location != PopupLocation.FixedAlwaysVisible)
		{
			if (popupOptions.ShowTooltipWhenNoContent != ShowTooltipCheckBox.Checked)
			{
				popupOptions.ShowTooltipWhenNoContent = ShowTooltipCheckBox.Checked;
				popupOptionChanged = true;
			}

			if (popupOptions.PinOnClick != PinOnClickCheckBox.Checked)
			{
				popupOptions.PinOnClick = PinOnClickCheckBox.Checked;
				popupOptionChanged = true;
			}

			if (tour.V3CompatibilityEnabled && popupOptions.PinOnClick && !popupOptions.LocationAllowsMouseOntoPopup)
			{
				// Only read the pin message when the pin option is checked and the mouse is not allowed
				// onto the popup. That's the only combination that will cause the message to be displayed.
				string pinMessage = PinMessage.Text;
				if (pinMessage.Length == 0)
				{
					// The user erased the message. Save as a single space so that we don't interpret this as the default message.
					pinMessage = " ";
				}
				if (popupOptions.PinMessage != pinMessage)
				{
					popupOptions.PinMessage = pinMessage;
					popupOptionChanged = true;
				}
			}

			PopupDelayType delayType = (PopupDelayType)int.Parse(DelayDropDown.SelectedValue);

			if (popupOptions.DelayType != delayType || popupOptions.Delay != validPopupDelay)
			{
				popupOptions.DelayType = delayType;
				popupOptions.Delay = validPopupDelay;
				popupOptionChanged = true;
			}

			int bestSideSequence = int.Parse(BestSideSequenceDropDown.SelectedValue);
			if (popupOptions.BestSideSequence != bestSideSequence)
			{
				popupOptions.BestSideSequence = bestSideSequence;
				popupOptionChanged = true;
			}
		}
	}

	private PopupLocation GetSelectedPopupLocation()
	{
		PopupLocation popupLocation;

		if (RadioButton1.Checked)
			popupLocation = PopupLocation.MarkerCenter;
		else if (RadioButton2.Checked)
			popupLocation = PopupLocation.MarkerEdgeInside;
		else if (RadioButton3.Checked)
			popupLocation = PopupLocation.MarkerEdgeOutside;
		else if (RadioButton4.Checked)
			popupLocation = PopupLocation.Mouse;
		else if (RadioButton5.Checked)
			popupLocation = PopupLocation.MouseFollow;
		else if (RadioButton6.Checked)
			popupLocation = PopupLocation.Fixed;
		else
			popupLocation = PopupLocation.FixedAlwaysVisible;

		return popupLocation;
	}

	private void SetSelectedPopupLocation(PopupLocation popupLocation)
	{
		RadioButton1.Checked = false;
		RadioButton2.Checked = false;
		RadioButton3.Checked = false;
		RadioButton4.Checked = false;
		RadioButton5.Checked = false;
		RadioButton6.Checked = false;
		RadioButton7.Checked = false;

		switch (popupLocation)
		{
			case PopupLocation.MarkerCenter:
				RadioButton1.Checked = true;
				break;
			
			case PopupLocation.MarkerEdgeInside:
				RadioButton2.Checked = true;
				break;
			
			case PopupLocation.MarkerEdgeOutside:
				RadioButton3.Checked = true;
				break;
			
			case PopupLocation.Mouse:
				RadioButton4.Checked = true;
				break;
			
			case PopupLocation.MouseFollow:
				RadioButton5.Checked = true;
				break;
			
			case PopupLocation.Fixed:
				RadioButton6.Checked = true;
				break;
			
			case PopupLocation.FixedAlwaysVisible:
				RadioButton7.Checked = true;
				break;
		}
	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		PopupLocation popupLocation = GetSelectedPopupLocation();
		
		if (popupLocation == PopupLocation.Fixed || popupLocation == PopupLocation.FixedAlwaysVisible)
		{
			validPopupSlideX = ValidateFieldInRange(PopupSlideX, -2400, 2400, PopupSlideXError);
			if (!pageValid)
				return;

			validPopupSlideY = ValidateFieldInRange(PopupSlideY, -2400, 2400, PopupSlideYError);
			if (!pageValid)
				return;
		}
		else
		{
			validMarkerOffset = ValidateFieldInRange(PopupMarkerOffset, -500, 500, PopupMarkerOffsetError);
			bool dontAllowMouseover = popupLocation == PopupLocation.MarkerEdgeOutside || popupLocation == PopupLocation.MouseFollow;
			if (pageValid && dontAllowMouseover && validMarkerOffset < 0)
			{
				SetErrorMessage(PopupMarkerOffsetError, "When the popup location does not allow the mouse to move onto the popup, the offset adjustment cannot be negative. A negative offset would cause the popup to overlap the marker and allow the mouse to move onto the popup.");
				fieldValid = false;
				SetFieldError(PopupMarkerOffsetError);
			}

			if (pageValid && popupLocation == PopupLocation.MouseFollow && ((PopupDelayType)int.Parse(DelayDropDown.SelectedValue)) == PopupDelayType.After)
			{
				SetErrorMessage(PopupDelayError, "An <i>After</i> delay is not allowed when the popup follows the mouse (because the popup closes as soon as the mouse moves off of the marker). Change the Delay option to None or Before.");
				fieldValid = false;
				SetFieldError(PopupDelayError);
			}

			if (!pageValid)
				return;
		}

		if (popupLocation != PopupLocation.FixedAlwaysVisible)
		{
			validPopupDelay = ValidateFieldInRange(PopupDelay, 0, 5000, PopupDelayError);
			if (!pageValid)
				return;
		}
	}

	private void ClearErrors()
	{
		ClearErrors(
			PopupSlideXError,
			PopupSlideYError,
			PopupDelayError);
	}
}
