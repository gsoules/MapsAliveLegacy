// Copyright (C) 2003-2010 AvantLogic Corporation

// We have to define these separately from their corresponding MemberPageActionId values
// since action values are not hard-coded in their enum. As such, they could change, but
// these values need to be stored in the DB and thus are not allowed to change.
public enum ImportType
{
	Undefined = 0,
	SlideContent = 1,
	Markers = 2,
	SlideImages = 3,
	Package = 4,
	Account = 5,
	Symbols = 6
}
