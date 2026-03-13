// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Diagnostics;

// Do not change these values -- they are known in the database. 
public enum SlideLayoutPattern
{
	HMMIT = 1,
	HMMTI = 2,
	HITMM = 3,
	HTIMM = 4,
	VMMIT = 5,
	VMMTI = 6,
	VITMM = 7,
	VTIMM = 8,

	HIIMT = 9,
	HIITM = 10,
	HMTII = 11,
	HTMII = 12,
	VIIMT = 13,
	VIITM = 14,
	VMTII = 15,
	VTMII = 16,

	HTTMI = 17,
	HTTIM = 18,
	HMITT = 19,
	HIMTT = 20,
	VTTMI = 21,
	VTTIM = 22,
	VMITT = 23,
	VIMTT = 24,

	HMMII = 25,
	HIIMM = 26,

	HMMTT = 27,
	HTTMM = 28,

	VMMTT = 29,
	VTTMM = 30,

	VMMII = 31,
	VIIMM = 32,

	HIITT = 33,
	HTTII = 34,

	VIITT = 35,
	VTTII = 36,

	HMM = 37,
	HII = 38,
	HTT = 39,

    HMIT = 40,
    HMTI = 41
}

public enum SlideLayoutFamily
{
	MapH,
	MapHI,
	MapHT,
	MapV,
	MapVI,
	MapVT,
	ImageH,
	ImageV,
	NoMapImageH,
	NoMapImageV,
	TextH,
	TextV,
	MapOnly,
	ImageOnly,
	TextOnly
}

public enum SplitterEdgeH
{
	MapTop,
	MapTopAndImageBottom,
	MapBottom,
	MapBottomAndImageTop,
	ImageTop,
	ImageBottom,
	None
}

public enum SplitterEdgeV
{
	MapLeft,
	MapLeftAndImageRight,
	MapRight,
	MapRightAndImageLeft,
	ImageLeft,
	ImageRight,
	None
}

public class SlideLayout
{
	private SlideLayoutMargin _margin;
	private Size _outerSize;
	private Rectangle imageArea;
	private Size innerSize;
	private Rectangle mapArea;
	private SlideLayoutPattern pattern;
	private SlideLayoutSpacing spacing;
	private SlideLayoutSplitters splitters;
	private Rectangle textArea;

	public SlideLayout(SlideLayout slideLayout)
	{
		// Copy the passed-in slideLayout.
		this.Pattern = slideLayout.pattern;
		this.OuterSize = slideLayout.OuterSize;
		this.Splitters = new SlideLayoutSplitters(slideLayout.splitters);
		this.Margin = new SlideLayoutMargin(slideLayout.Margin);
		this.Spacing = new SlideLayoutSpacing(slideLayout.Spacing);
		this.ImageArea = slideLayout.imageArea;
		this.TextArea = slideLayout.textArea;
		this.MapArea = slideLayout.mapArea;
	}

	public SlideLayout(
		SlideLayoutPattern pattern,
		Size outerSize,
		SlideLayoutSplitters splitters,
		SlideLayoutMargin margin,
		SlideLayoutSpacing spacing)
	{
		// Create a new slideLayout.
		this.pattern = pattern;
		this.OuterSize = outerSize;
		this.Splitters = splitters;
		this.Margin = margin;
		this.Spacing = spacing;

		// Intialize the layout's area sizes. If both splitters are set to -1, this layout
		// is not active (e.g. if it's a popup layout, the tour page is using fixed slides).
		// If and when the user switchs to this layout (e.g. from fixed to popup) the splitters
		// will get set. Note that if we ran SetLayoutAreaSizes with negative splitters, the
		// logic there would adjust them to valid minimum positions.
		if (splitters.H != -1 || splitters.V != -1)
			SetLayoutAreaSizes();
	}

	public static bool operator ==(SlideLayout s1, SlideLayout s2)
	{
		if ((object)s1 == null && (object)s2 == null)
			return true;

		if ((object)s1 == null || (object)s2 == null)
			return false;

		return
			s1._margin == s2._margin &&
			s1._outerSize == s2._outerSize &&
			s1.pattern == s2.pattern &&
			s1.splitters == s2.splitters &&
			s1.spacing == s2.spacing;
	}

	public static bool operator !=(SlideLayout s1, SlideLayout s2)
	{
		return !(s1 == s2);
	}

	private void CalculateInnerSize()
	{
		int width = _outerSize.Width - _margin.Left - _margin.Right;
		int height = _outerSize.Height - _margin.Top - _margin.Bottom;
		innerSize = new Size(width, height);
	}

	public override bool Equals(object o)
	{
		return true;
	}

	public SlideLayoutFamily Family
	{
		get { return GetFamily(pattern); }
	}

	public override int GetHashCode()
	{
		// This method is only here to avoid the compiler warning that occurs if Equals is overridden
		// without GetHashCode being overridden. We override Equals because we are overriding ==.
		return base.GetHashCode();
	}

	public static bool GetHasHorizontalSplitter(SlideLayoutPattern slideLayoutPattern)
	{
		switch (slideLayoutPattern)
		{
			case SlideLayoutPattern.VMMTT:
			case SlideLayoutPattern.VTTMM:
			case SlideLayoutPattern.VMMII:
			case SlideLayoutPattern.VIIMM:
			case SlideLayoutPattern.VIITT:
			case SlideLayoutPattern.VTTII:
			case SlideLayoutPattern.HII:
			case SlideLayoutPattern.HTT:
			case SlideLayoutPattern.HMM:
				return false;

			default:
				return true;
		}
	}

	public static bool GetHasVerticalSplitter(SlideLayoutPattern slideLayoutPattern)
	{
		switch (slideLayoutPattern)
		{
			case SlideLayoutPattern.HMMTT:
			case SlideLayoutPattern.HTTMM:
			case SlideLayoutPattern.HMMII:
			case SlideLayoutPattern.HIIMM:
			case SlideLayoutPattern.HIITT:
			case SlideLayoutPattern.HTTII:
			case SlideLayoutPattern.HII:
			case SlideLayoutPattern.HTT:
			case SlideLayoutPattern.HMM:
			case SlideLayoutPattern.HMIT:
			case SlideLayoutPattern.HMTI:
				return false;

			default:
				return true;
		}
	}

	public static bool GetHasImageArea(SlideLayoutPattern slideLayoutPattern)
	{
		return slideLayoutPattern.ToString().Contains("I");
	}

	public static bool GetHasMapArea(SlideLayoutPattern slideLayoutPattern)
	{
        switch (slideLayoutPattern)
        {
            case SlideLayoutPattern.HIITT:
            case SlideLayoutPattern.HTTII:
            case SlideLayoutPattern.VIITT:
            case SlideLayoutPattern.VTTII:
            case SlideLayoutPattern.HII:
            case SlideLayoutPattern.HTT:
                return false;

            default:
                return true;
        }
    }

    public static bool GetHasTextArea(SlideLayoutPattern slideLayoutPattern)
	{
		return slideLayoutPattern.ToString().Contains("T");
	}

	public static SlideLayoutFamily GetFamily(SlideLayoutPattern slideLayoutPattern)
	{
		SlideLayoutFamily slideLayoutFamily;

		switch (slideLayoutPattern)
		{
			case SlideLayoutPattern.HMMII:
			case SlideLayoutPattern.HIIMM:
				slideLayoutFamily = SlideLayoutFamily.MapHI;
				break;

			case SlideLayoutPattern.HMMTT:
			case SlideLayoutPattern.HTTMM:
				slideLayoutFamily = SlideLayoutFamily.MapHT;
				break;

			case SlideLayoutPattern.HMMIT:
			case SlideLayoutPattern.HMMTI:
			case SlideLayoutPattern.HITMM:
			case SlideLayoutPattern.HTIMM:
				slideLayoutFamily = SlideLayoutFamily.MapH;
				break;

			case SlideLayoutPattern.VMMII:
			case SlideLayoutPattern.VIIMM:
				slideLayoutFamily = SlideLayoutFamily.MapVI;
				break;

			case SlideLayoutPattern.VMMTT:
			case SlideLayoutPattern.VTTMM:
				slideLayoutFamily = SlideLayoutFamily.MapVT;
				break;

			case SlideLayoutPattern.VMMIT:
			case SlideLayoutPattern.VMMTI:
			case SlideLayoutPattern.VITMM:
			case SlideLayoutPattern.VTIMM:
				slideLayoutFamily = SlideLayoutFamily.MapV;
				break;

			case SlideLayoutPattern.HIIMT:
			case SlideLayoutPattern.HIITM:
			case SlideLayoutPattern.HMTII:
			case SlideLayoutPattern.HTMII:
				slideLayoutFamily = SlideLayoutFamily.ImageH;
				break;

			case SlideLayoutPattern.VMTII:
			case SlideLayoutPattern.VIIMT:
			case SlideLayoutPattern.VIITM:
			case SlideLayoutPattern.VTMII:
				slideLayoutFamily = SlideLayoutFamily.ImageV;
				break;

			case SlideLayoutPattern.HIITT:
			case SlideLayoutPattern.HTTII:
				slideLayoutFamily = SlideLayoutFamily.NoMapImageH;
				break;

			case SlideLayoutPattern.VIITT:
			case SlideLayoutPattern.VTTII:
				slideLayoutFamily = SlideLayoutFamily.NoMapImageV;
				break;

			case SlideLayoutPattern.HMITT:
			case SlideLayoutPattern.HIMTT:
			case SlideLayoutPattern.HTTMI:
			case SlideLayoutPattern.HTTIM:
				slideLayoutFamily = SlideLayoutFamily.TextH;
				break;

			case SlideLayoutPattern.VTTMI:
			case SlideLayoutPattern.VTTIM:
			case SlideLayoutPattern.VMITT:
			case SlideLayoutPattern.VIMTT:
				slideLayoutFamily = SlideLayoutFamily.TextV;
				break;

			case SlideLayoutPattern.HMM:
				slideLayoutFamily = SlideLayoutFamily.MapOnly;
				break;
			
			case SlideLayoutPattern.HII:
				slideLayoutFamily = SlideLayoutFamily.ImageOnly;
				break;
			
			case SlideLayoutPattern.HTT:
				slideLayoutFamily = SlideLayoutFamily.TextOnly;
				break;
			
			case SlideLayoutPattern.HMIT:
				slideLayoutFamily = SlideLayoutFamily.MapVI;
				break;
			
			case SlideLayoutPattern.HMTI:
				slideLayoutFamily = SlideLayoutFamily.MapVT;
				break;

			default:
				Debug.Fail("Unexpected slide layout type " + slideLayoutPattern.ToString());
				slideLayoutFamily = SlideLayoutFamily.TextOnly;
				break;
		}

		return slideLayoutFamily;
	}

	public static SplitterEdgeH GetSplitterEdgeH(SlideLayoutPattern pattern)
	{
		switch (pattern)
		{
			case SlideLayoutPattern.HMMIT:
				return SplitterEdgeH.MapBottom;

			case SlideLayoutPattern.HMMTI:
				return SplitterEdgeH.MapBottom;

			case SlideLayoutPattern.HMMII:
				return SplitterEdgeH.MapBottom;

			case SlideLayoutPattern.HMMTT:
				return SplitterEdgeH.MapBottom;

			case SlideLayoutPattern.HMM:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.VMMIT:
				return SplitterEdgeH.ImageBottom;

			case SlideLayoutPattern.VMMII:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.VMMTT:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.VMMTI:
				return SplitterEdgeH.ImageTop;

			case SlideLayoutPattern.VITMM:
				return SplitterEdgeH.ImageBottom;

			case SlideLayoutPattern.VIIMM:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.VTTMM:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.VTIMM:
				return SplitterEdgeH.ImageTop;

			case SlideLayoutPattern.HITMM:
				return SplitterEdgeH.MapTop;

			case SlideLayoutPattern.HTIMM:
				return SplitterEdgeH.MapTop;

			case SlideLayoutPattern.HIIMM:
				return SplitterEdgeH.MapTop;

			case SlideLayoutPattern.HTTMM:
				return SplitterEdgeH.MapTop;

			case SlideLayoutPattern.VMTII:
				return SplitterEdgeH.MapBottom;

			case SlideLayoutPattern.HMITT:
				return SplitterEdgeH.MapBottom;

			case SlideLayoutPattern.VIIMT:
				return SplitterEdgeH.MapBottom;

			case SlideLayoutPattern.HIMTT:
				return SplitterEdgeH.MapBottom;

			case SlideLayoutPattern.VIITT:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.VTTII:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.HII:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.HTT:
				return SplitterEdgeH.None;

			case SlideLayoutPattern.HIITT:
				return SplitterEdgeH.ImageBottom;

			case SlideLayoutPattern.HTTII:
				return SplitterEdgeH.ImageTop;

			case SlideLayoutPattern.VIITM:
				return SplitterEdgeH.MapTop;

			case SlideLayoutPattern.VTMII:
				return SplitterEdgeH.MapTop;

			case SlideLayoutPattern.HIIMT:
				return SplitterEdgeH.MapTopAndImageBottom;

			case SlideLayoutPattern.HIITM:
				return SplitterEdgeH.MapTopAndImageBottom;

			case SlideLayoutPattern.HMTII:
				return SplitterEdgeH.MapBottomAndImageTop;

			case SlideLayoutPattern.HTMII:
				return SplitterEdgeH.MapBottomAndImageTop;

			case SlideLayoutPattern.VTTMI:
				return SplitterEdgeH.MapBottomAndImageTop;

			case SlideLayoutPattern.VTTIM:
				return SplitterEdgeH.MapTopAndImageBottom;

			case SlideLayoutPattern.VMITT:
				return SplitterEdgeH.MapBottomAndImageTop;

			case SlideLayoutPattern.VIMTT:
				return SplitterEdgeH.MapTopAndImageBottom;

			case SlideLayoutPattern.HTTMI:
				return SplitterEdgeH.MapTop;

			case SlideLayoutPattern.HTTIM:
				return SplitterEdgeH.MapTop;

            case SlideLayoutPattern.HMIT:
                return SplitterEdgeH.MapBottom;

            case SlideLayoutPattern.HMTI:
                return SplitterEdgeH.MapBottom;

            default:
				Debug.Fail("Unexpected slide layout type " + pattern.ToString());
				return SplitterEdgeH.None;
		}
	}

	public static SplitterEdgeV GetSplitterEdgeV(SlideLayoutPattern pattern)
	{
		switch (pattern)
		{
			case SlideLayoutPattern.HMMIT:
				return SplitterEdgeV.ImageRight;

			case SlideLayoutPattern.HMMTI:
				return SplitterEdgeV.ImageLeft;

			case SlideLayoutPattern.HMMII:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.HMMTT:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.HMM:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.VMMIT:
				return SplitterEdgeV.MapRight;

			case SlideLayoutPattern.VMMII:
				return SplitterEdgeV.MapRight;

			case SlideLayoutPattern.VMMTT:
				return SplitterEdgeV.MapRight;

			case SlideLayoutPattern.VMMTI:
				return SplitterEdgeV.MapRight;

			case SlideLayoutPattern.VITMM:
				return SplitterEdgeV.MapLeft;

			case SlideLayoutPattern.VIIMM:
				return SplitterEdgeV.MapLeft;

			case SlideLayoutPattern.VTTMM:
				return SplitterEdgeV.MapLeft;

			case SlideLayoutPattern.VTIMM:
				return SplitterEdgeV.MapLeft;

			case SlideLayoutPattern.HITMM:
				return SplitterEdgeV.ImageRight;

			case SlideLayoutPattern.HTIMM:
				return SplitterEdgeV.ImageLeft;

			case SlideLayoutPattern.HIIMM:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.HTTMM:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.VMTII:
				return SplitterEdgeV.MapRightAndImageLeft;

			case SlideLayoutPattern.HMITT:
				return SplitterEdgeV.MapRightAndImageLeft;

			case SlideLayoutPattern.VIIMT:
				return SplitterEdgeV.MapLeftAndImageRight;

			case SlideLayoutPattern.HIMTT:
				return SplitterEdgeV.MapLeftAndImageRight;

			case SlideLayoutPattern.VIITT:
				return SplitterEdgeV.ImageRight;

			case SlideLayoutPattern.VTTII:
				return SplitterEdgeV.ImageLeft;

			case SlideLayoutPattern.HII:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.HTT:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.HIITT:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.HTTII:
				return SplitterEdgeV.None;

			case SlideLayoutPattern.VIITM:
				return SplitterEdgeV.MapLeftAndImageRight;

			case SlideLayoutPattern.VTMII:
				return SplitterEdgeV.MapRightAndImageLeft;

			case SlideLayoutPattern.HIIMT:
				return SplitterEdgeV.MapRight;

			case SlideLayoutPattern.HIITM:
				return SplitterEdgeV.MapLeft;

			case SlideLayoutPattern.HMTII:
				return SplitterEdgeV.MapRight;

			case SlideLayoutPattern.HTMII:
				return SplitterEdgeV.MapLeft;

			case SlideLayoutPattern.VTTMI:
				return SplitterEdgeV.MapLeft;

			case SlideLayoutPattern.VTTIM:
				return SplitterEdgeV.MapLeft;

			case SlideLayoutPattern.VMITT:
				return SplitterEdgeV.MapRight;

			case SlideLayoutPattern.VIMTT:
				return SplitterEdgeV.MapRight;

			case SlideLayoutPattern.HTTMI:
				return SplitterEdgeV.MapRightAndImageLeft;

			case SlideLayoutPattern.HTTIM:
				return SplitterEdgeV.MapLeftAndImageRight;

            case SlideLayoutPattern.HMIT:
                return SplitterEdgeV.None;

            case SlideLayoutPattern.HMTI:
                return SplitterEdgeV.None;

            default:
				Debug.Fail("Unexpected slide layout type " + pattern.ToString());
				return SplitterEdgeV.None;
		}
	}

	public static bool HasSideBySideTextAndImage(SlideLayoutPattern pattern)
	{
		switch (pattern)
		{
			case SlideLayoutPattern.HMMIT:
			case SlideLayoutPattern.HMMTI:
			case SlideLayoutPattern.HITMM:
			case SlideLayoutPattern.HTIMM:
			case SlideLayoutPattern.VIITT:
			case SlideLayoutPattern.VTTII:
				return true;

			default:
				return false;
		}
	}

	public bool HasHorizontalSplitter
	{
		get { return GetHasHorizontalSplitter(pattern); }
	}

	public bool HasImageArea
	{
		get { return GetHasImageArea(pattern); }
	}

	public bool HasMapArea
	{
		get { return GetHasMapArea(pattern); }
	}

	public bool HasTextArea
	{
		get { return GetHasTextArea(pattern); }
	}

	public bool HasVerticalSplitter
	{
		get { return GetHasVerticalSplitter(pattern); }
	}

	public Rectangle ImageArea
	{
		get { return imageArea; }
		set { imageArea = value; }
	}

	public Size InnerSize
	{
		get
		{
			System.Diagnostics.Debug.Assert(innerSize != Size.Empty, "innerSize is empty");
			return innerSize;
		}
	}

    public static bool IsDeprecatedLayout(SlideLayoutPattern slideLayoutPattern, TourPage tourPage)
    {
        if (slideLayoutPattern >= SlideLayoutPattern.HIIMT && slideLayoutPattern <= SlideLayoutPattern.VIMTT)
            return true;

        if (slideLayoutPattern == SlideLayoutPattern.HITMM ||
            slideLayoutPattern == SlideLayoutPattern.HTIMM ||
            slideLayoutPattern == SlideLayoutPattern.HIIMM ||
            slideLayoutPattern == SlideLayoutPattern.HTTMM)
            return true;

        if (tourPage.IsDataSheet)
            return false;

        if (slideLayoutPattern == SlideLayoutPattern.VIITT || slideLayoutPattern == SlideLayoutPattern.VTTII)
            return true;

        return false;
    }

    public static bool IsStackedResponsiveLayout(SlideLayoutPattern slideLayoutPattern)
    {
        return slideLayoutPattern == SlideLayoutPattern.HMIT || slideLayoutPattern == SlideLayoutPattern.HMTI;
    }

    public bool IsInvertedSplitterH
	{
		get
		{
			bool below = false;

			switch (SplitterEdgeH)
			{
				case SplitterEdgeH.MapBottom:
				case SplitterEdgeH.ImageBottom:
				case SplitterEdgeH.MapTopAndImageBottom:
					below = false;
					break;

				case SplitterEdgeH.MapTop:
				case SplitterEdgeH.ImageTop:
				case SplitterEdgeH.MapBottomAndImageTop:
					below = true;
					break;
			}
			return below;
		}
	}

	public bool IsInvertedSplitterV
	{
		get
		{
			bool right = false;

			switch (SplitterEdgeV)
			{
				case SplitterEdgeV.MapRight:
				case SplitterEdgeV.MapLeftAndImageRight:
				case SplitterEdgeV.ImageRight:
					right = false;
					break;

				case SplitterEdgeV.MapLeft:
				case SplitterEdgeV.MapRightAndImageLeft:
				case SplitterEdgeV.ImageLeft:
					right = true;
					break;
			}
			return right;
		}
	}

	public virtual Rectangle MapArea
	{
		get { return mapArea; }
		set { mapArea = value; }
	}

	public SlideLayoutMargin Margin
	{
		get { return _margin; }
		set
		{
			_margin = value;
			CalculateInnerSize();
		}
	}

	public string MinNonMapWidthMeaning
	{
		get
		{
			string meaning = string.Empty;

			if (HasVerticalSplitter)
			{
				switch (Family)
				{
					case SlideLayoutFamily.MapV:
					case SlideLayoutFamily.MapVI:
					case SlideLayoutFamily.MapVT:
					case SlideLayoutFamily.MapH:
					case SlideLayoutFamily.MapHT:
					case SlideLayoutFamily.ImageH:
					case SlideLayoutFamily.TextV:
					case SlideLayoutFamily.NoMapImageV:
						meaning = "Text Area Width";
						break;

					case SlideLayoutFamily.MapHI:
					case SlideLayoutFamily.ImageV:
					case SlideLayoutFamily.TextH:
						meaning = "Image Area Width";
						break;
				}
			}

			return meaning;
		}
	}

	public string MinNonMapHeightMeaning
	{
		get
		{
			string meaning = string.Empty;

			if (HasHorizontalSplitter)
			{
				switch (Family)
				{
					case SlideLayoutFamily.MapV:
					case SlideLayoutFamily.MapVI:
					case SlideLayoutFamily.MapVT:
					case SlideLayoutFamily.MapHT:
					case SlideLayoutFamily.ImageV:
					case SlideLayoutFamily.TextH:
					case SlideLayoutFamily.NoMapImageH:
					case SlideLayoutFamily.MapH:
						meaning = "Text Area Height";
						break;

					case SlideLayoutFamily.MapHI:
					case SlideLayoutFamily.ImageH:
					case SlideLayoutFamily.TextV:
						meaning = "Image Area Height";
						break;
				}
			}

			return meaning;
		}
	}
	
	public Size OuterSize
	{
		get { return _outerSize; }
		set
		{
			_outerSize = value;
			CalculateInnerSize();
		}
	}

	public SlideLayoutPattern Pattern
	{
		get { return pattern; }
		set { pattern = value; }
	}

	private void SetLayoutAreaSizes()
	{
		SlideLayoutAreas areas;

		switch (pattern)
		{
			case SlideLayoutPattern.HMMIT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomSplit, this);
				MapArea = areas.Top;
				ImageArea = areas.BottomLeft;
				TextArea = areas.BottomRight;
				break;

			case SlideLayoutPattern.HMMTI:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomSplit, this);
				MapArea = areas.Top;
				ImageArea = areas.BottomRight;
				TextArea = areas.BottomLeft;
				break;

			case SlideLayoutPattern.HMMII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomWide, this);
				MapArea = areas.Top;
				ImageArea = areas.Bottom;
				TextArea = areas.Empty;
				break;

			case SlideLayoutPattern.HMMTT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomWide, this);
				MapArea = areas.Top;
				ImageArea = areas.Empty;
				TextArea = areas.Bottom;
				break;

			case SlideLayoutPattern.HMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.SingleArea, this);
				MapArea = areas.Top;
				ImageArea = areas.Empty;
				TextArea = areas.Empty;
				break;

			case SlideLayoutPattern.VMMIT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightSplit, this);
				MapArea = areas.Left;
				ImageArea = areas.TopRight;
				TextArea = areas.BottomRight;
				break;

			case SlideLayoutPattern.VMMII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightTall, this);
				MapArea = areas.Left;
				ImageArea = areas.Right;
				TextArea = areas.Empty;
				break;

			case SlideLayoutPattern.VMMTT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightTall, this);
				MapArea = areas.Left;
				ImageArea = areas.Empty;
				TextArea = areas.Right;
				break;

			case SlideLayoutPattern.VMMTI:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightSplit, this);
				MapArea = areas.Left;
				ImageArea = areas.BottomRight;
				TextArea = areas.TopRight;
				break;

			case SlideLayoutPattern.VITMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftSplit_RightTall, this);
				MapArea = areas.Right;
				ImageArea = areas.TopLeft;
				TextArea = areas.BottomLeft;
				break;

			case SlideLayoutPattern.VIIMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightTall, this);
				MapArea = areas.Right;
				ImageArea = areas.Left;
				TextArea = areas.Empty;
				break;

			case SlideLayoutPattern.VTTMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightTall, this);
				MapArea = areas.Right;
				ImageArea = areas.Empty;
				TextArea = areas.Left;
				break;

			case SlideLayoutPattern.VTIMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftSplit_RightTall, this);
				MapArea = areas.Right;
				ImageArea = areas.BottomLeft;
				TextArea = areas.TopLeft;
				break;

			case SlideLayoutPattern.HITMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopSplit_BottomWide, this);
				MapArea = areas.Bottom;
				ImageArea = areas.TopLeft;
				TextArea = areas.TopRight;
				break;

			case SlideLayoutPattern.HTIMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopSplit_BottomWide, this);
				MapArea = areas.Bottom;
				ImageArea = areas.TopRight;
				TextArea = areas.TopLeft;
				break;

			case SlideLayoutPattern.HIIMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomWide, this);
				MapArea = areas.Bottom;
				ImageArea = areas.Top;
				TextArea = areas.Empty;
				break;

			case SlideLayoutPattern.HTTMM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomWide, this);
				MapArea = areas.Bottom;
				ImageArea = areas.Empty;
				TextArea = areas.Top;
				break;

			case SlideLayoutPattern.VMTII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftSplit_RightTall, this);
				MapArea = areas.TopLeft;
				ImageArea = areas.Right;
				TextArea = areas.BottomLeft;
				break;

			case SlideLayoutPattern.HMITT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopSplit_BottomWide, this);
				MapArea = areas.TopLeft;
				ImageArea = areas.TopRight;
				TextArea = areas.Bottom;
				break;

			case SlideLayoutPattern.VIIMT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightSplit, this);
				MapArea = areas.TopRight;
				ImageArea = areas.Left;
				TextArea = areas.BottomRight;
				break;

			case SlideLayoutPattern.HIMTT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopSplit_BottomWide, this);
				MapArea = areas.TopRight;
				ImageArea = areas.TopLeft;
				TextArea = areas.Bottom;
				break;

			case SlideLayoutPattern.VIITT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightTall, this);
				MapArea = areas.Empty;
				ImageArea = areas.Left;
				TextArea = areas.Right;
				break;

			case SlideLayoutPattern.VTTII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightTall, this);
				MapArea = areas.Empty;
				ImageArea = areas.Right;
				TextArea = areas.Left;
				break;

			case SlideLayoutPattern.HII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.SingleArea, this);
				MapArea = areas.Empty;
				ImageArea = areas.Top;
				TextArea = areas.Empty;
				break;

			case SlideLayoutPattern.HTT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.SingleArea, this);
				MapArea = areas.Empty;
				ImageArea = areas.Empty;
				TextArea = areas.Top;
				break;

			case SlideLayoutPattern.HIITT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomWide, this);
				MapArea = areas.Empty;
				ImageArea = areas.Top;
				TextArea = areas.Bottom;
				break;

			case SlideLayoutPattern.HTTII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomWide, this);
				MapArea = areas.Empty;
				ImageArea = areas.Bottom;
				TextArea = areas.Top;
				break;

			case SlideLayoutPattern.VIITM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightSplit, this);
				MapArea = areas.BottomRight;
				ImageArea = areas.Left;
				TextArea = areas.TopRight;
				break;

			case SlideLayoutPattern.VTMII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftSplit_RightTall, this);
				MapArea = areas.BottomLeft;
				ImageArea = areas.Right;
				TextArea = areas.TopLeft;
				break;

			case SlideLayoutPattern.HIIMT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomSplit, this);
				MapArea = areas.BottomLeft;
				ImageArea = areas.Top;
				TextArea = areas.BottomRight;
				break;

			case SlideLayoutPattern.HIITM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomSplit, this);
				MapArea = areas.BottomRight;
				ImageArea = areas.Top;
				TextArea = areas.BottomLeft;
				break;

			case SlideLayoutPattern.HMTII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopSplit_BottomWide, this);
				MapArea = areas.TopLeft;
				ImageArea = areas.Bottom;
				TextArea = areas.TopRight;
				break;

			case SlideLayoutPattern.HTMII:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopSplit_BottomWide, this);
				MapArea = areas.TopRight;
				ImageArea = areas.Bottom;
				TextArea = areas.TopLeft;
				break;

			case SlideLayoutPattern.VTTMI:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightSplit, this);
				MapArea = areas.TopRight;
				ImageArea = areas.BottomRight;
				TextArea = areas.Left;
				break;

			case SlideLayoutPattern.VTTIM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftTall_RightSplit, this);
				MapArea = areas.BottomRight;
				ImageArea = areas.TopRight;
				TextArea = areas.Left;
				break;

			case SlideLayoutPattern.VMITT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftSplit_RightTall, this);
				MapArea = areas.TopLeft;
				ImageArea = areas.BottomLeft;
				TextArea = areas.Right;
				break;

			case SlideLayoutPattern.VIMTT:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Vertical_LeftSplit_RightTall, this);
				MapArea = areas.BottomLeft;
				ImageArea = areas.TopLeft;
				TextArea = areas.Right;
				break;

			case SlideLayoutPattern.HTTMI:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomSplit, this);
				MapArea = areas.BottomLeft;
				ImageArea = areas.BottomRight;
				TextArea = areas.Top;
				break;

			case SlideLayoutPattern.HTTIM:
				areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomSplit, this);
				MapArea = areas.BottomRight;
				ImageArea = areas.BottomLeft;
				TextArea = areas.Top;
				break;

            case SlideLayoutPattern.HMIT:
                areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomWide, this);
                MapArea = areas.Top;
                ImageArea = areas.Bottom;
                TextArea = areas.Bottom;
                break;

            case SlideLayoutPattern.HMTI:
                areas = new SlideLayoutAreas(SlideLayoutAreasType.Horizontal_TopWide_BottomWide, this);
                MapArea = areas.Top;
                ImageArea = areas.Bottom;
                TextArea = areas.Bottom;
                break;

            default:
				break;
		}
	}

	public void SetNewMargin(SlideLayoutMargin value)
	{
		Margin = value;
		SetLayoutAreaSizes();
	}

	public void SetNewOuterSize(Size value)
	{
		OuterSize = value;
		SetLayoutAreaSizes();
	}

	public void SetNewSpacing(SlideLayoutSpacing value)
	{
		Spacing = value;
		SetLayoutAreaSizes();
	}

	public void SetNewSplitters(SlideLayoutSplitters value)
	{
		Splitters = value;
		SetLayoutAreaSizes();
	}

	public SlideLayoutSpacing Spacing
	{
		get { return spacing; }

		set	{ spacing = value; }
	}

	public SlideLayoutSplitters Splitters
	{
		get { return splitters; }
		set	{ splitters = value; }
	}

	public SplitterEdgeH SplitterEdgeH
	{
		get { return SlideLayout.GetSplitterEdgeH(pattern); }
	}

	public SplitterEdgeV SplitterEdgeV
	{
		get { return SlideLayout.GetSplitterEdgeV(pattern); }
	}

	public string SplitterMeaning(SplitterEdgeH splitterEdgeH)
	{
		string meaning = null;
		switch (splitterEdgeH)
		{
			case SplitterEdgeH.MapTop:
			case SplitterEdgeH.MapBottom:
				meaning = "Map Area Height";
				break;
			case SplitterEdgeH.MapTopAndImageBottom:
			case SplitterEdgeH.MapBottomAndImageTop:
			case SplitterEdgeH.ImageTop:
			case SplitterEdgeH.ImageBottom:
				meaning = "Image Area Height";
				break;
		}
		return meaning;
	}

	public string SplitterMeaning(SplitterEdgeV splitterEdgeV)
	{
		string meaning = null;

		switch (splitterEdgeV)
		{
			case SplitterEdgeV.MapLeft:
			case SplitterEdgeV.MapRight:
				meaning = "Map Area Width";
				break;
			case SplitterEdgeV.MapLeftAndImageRight:
			case SplitterEdgeV.MapRightAndImageLeft:
			case SplitterEdgeV.ImageLeft:
			case SplitterEdgeV.ImageRight:
				meaning = "Image Area Width";
				break;
		}

		return meaning;
	}

	public int SplitterAreaHeight
	{
		get
		{
			// Return the height of the area controlled by the horizontal splitter.
			int rawValue = Splitters.H;
			return IsInvertedSplitterH ? InnerSize.Height - rawValue - Spacing.H : rawValue;
		}
	}

    public double SplitterAreaHeightPercent
    {
        get
        {
            double percent = (double)Splitters.H / ((double)InnerSize.Height - Spacing.H);
            return Math.Round(percent * 100, 1);
        }
    }

    public int SplitterAreaWidth
	{
		get
		{
			// Return the width of the area controlled by the vertical splitter.
			int rawValue = Splitters.V;
			return IsInvertedSplitterV ? InnerSize.Width - rawValue - Spacing.V : rawValue;
		}
	}

	public double SplitterAreaWidthPercent
	{
		get 
        {
            double percent = (double)Splitters.V / ((double)InnerSize.Width - Spacing.V);
            return Math.Round(percent * 100, 1);
        }
    }

	public Rectangle TextArea
	{
		get { return textArea; }
		set { textArea = value; }
	}

	public static void TranslateSplitters(Tour tour, SlideLayout oldSlideLayout, SlideLayoutPattern newSlideLayoutPattern, out int newSplitterH, out int newSplitterV)
	{
		// This method translates one layout to another using the sliders positions and usage of the "old"
		// layout to position the sliders for the new layout.  A slider can have just one element that it
		// controls -- either the map or the image.  When translating, the goal is for the elements of the
		// new layout to be the same size as in the old layout.

		Size innerSize = oldSlideLayout.InnerSize;
		int height = innerSize.Height;
		int width = innerSize.Width;

		if (tour.V3CompatibilityEnabled)
        {
            SplitterEdgeH oldSplitterEdgeH = oldSlideLayout.SplitterEdgeH;
		    SplitterEdgeV oldSplitterEdgeV = oldSlideLayout.SplitterEdgeV;
		    SplitterEdgeH newSplitterEdgeH = SlideLayout.GetSplitterEdgeH(newSlideLayoutPattern);
		    SplitterEdgeV newSplitterEdgeV = SlideLayout.GetSplitterEdgeV(newSlideLayoutPattern);

		    newSplitterH = oldSlideLayout.Splitters.H;
		    newSplitterV = oldSlideLayout.Splitters.V;
		    int spacingH = oldSlideLayout.Spacing.H;
		    int spacingV = oldSlideLayout.Spacing.V;

		    // Translate the horizontal slider.  If, for example, it is used to control the map's height
		    // from the bottom in the old layout, but is used to control the map's height from the top in
		    // the new layout (because the map's location has switched from top to bottom) move the slider
		    // to the location of the top of the map in the new layout.
		    if (oldSplitterEdgeH == SplitterEdgeH.MapBottom && newSplitterEdgeH == SplitterEdgeH.MapTop ||
			    oldSplitterEdgeH == SplitterEdgeH.MapTop && newSplitterEdgeH == SplitterEdgeH.MapBottom ||
			    oldSplitterEdgeH == SplitterEdgeH.ImageBottom && newSplitterEdgeH == SplitterEdgeH.ImageTop ||
			    oldSplitterEdgeH == SplitterEdgeH.ImageTop && newSplitterEdgeH == SplitterEdgeH.ImageBottom ||
			    oldSplitterEdgeH == SplitterEdgeH.MapBottomAndImageTop && newSplitterEdgeH == SplitterEdgeH.MapTopAndImageBottom ||
			    oldSplitterEdgeH == SplitterEdgeH.MapTopAndImageBottom && newSplitterEdgeH == SplitterEdgeH.MapBottomAndImageTop)
		    {
			    newSplitterH = height - spacingH - oldSlideLayout.Splitters.H;
		    }

		    // Translate the vertical slider.
		    if (oldSplitterEdgeV == SplitterEdgeV.MapLeft && newSplitterEdgeV == SplitterEdgeV.MapRight ||
			    oldSplitterEdgeV == SplitterEdgeV.MapRight && newSplitterEdgeV == SplitterEdgeV.MapLeft ||
			    oldSplitterEdgeV == SplitterEdgeV.ImageLeft && newSplitterEdgeV == SplitterEdgeV.ImageRight ||
			    oldSplitterEdgeV == SplitterEdgeV.ImageRight && newSplitterEdgeV == SplitterEdgeV.ImageLeft ||
			    oldSplitterEdgeV == SplitterEdgeV.MapLeftAndImageRight && newSplitterEdgeV == SplitterEdgeV.MapRightAndImageLeft ||
			    oldSplitterEdgeV == SplitterEdgeV.MapRightAndImageLeft && newSplitterEdgeV == SplitterEdgeV.MapLeftAndImageRight)
		    {
			    newSplitterV = width - spacingV - oldSlideLayout.Splitters.V;
		    }
        }
        else
        {
            // Place the splitters halfway up/down or left/right except for the splitter between image over text or text over
            // image. For those, use an 75/25 split so that the image area is very tall. This way, in most cases, the actual
            // popup will display with the image as tall as it needs to be and the text will adjust to be adjacent to the image
            // with no gap. Using the default of 50/50 in those cases is likely to cause the image to scale down to fit and
            // cause the user to wonder why the image is so small.
            double percentH;
            if (newSlideLayoutPattern == SlideLayoutPattern.HIITT ||
                newSlideLayoutPattern == SlideLayoutPattern.VMMIT ||
                newSlideLayoutPattern == SlideLayoutPattern.VITMM)
                percentH = 0.75;
            else if (newSlideLayoutPattern == SlideLayoutPattern.HTTII ||
                newSlideLayoutPattern == SlideLayoutPattern.VMMTI ||
                newSlideLayoutPattern == SlideLayoutPattern.VTIMM)
                percentH = 0.25;
            else
                percentH = 0.5;
            
            newSplitterH = (int)((double)height * percentH);
            newSplitterV = (int)((double)width * 0.5);
        }
    }
}