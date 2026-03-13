// Copyright (C) 2003-2005 AvantLogic Corporation
using System;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RuntimeException : ApplicationException
	{
		public RuntimeException(string message) : base(message) {}
		public RuntimeException(string message, Exception ex) : base(message, ex) {}
	}
}
