// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Drawing;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseWebPageImageValue
	{
		private BaseFileDescriptor fileDescriptor;
		private Point positionInStage;
		private int viewId;

		public BaseWebPageImageValue(BaseFileDescriptor fileDescriptor, Point positionInStage, int viewId)
		{
			this.fileDescriptor = fileDescriptor;
			this.positionInStage = positionInStage;
			this.viewId = viewId;
		}

		#region ===== Properties ========================================================

		public BaseFileDescriptor FileDescriptor
		{
			get { return fileDescriptor; }
		}

		public int ViewId
		{
			get { return viewId; }
		}
		#endregion

		#region ===== Public ============================================================

		public Point PositionInStage(int themeId)
		{
			return positionInStage;
		}
		#endregion

		#region ===== Protected =========================================================
		#endregion

		#region ===== Private ===========================================================
		#endregion
	}
}
