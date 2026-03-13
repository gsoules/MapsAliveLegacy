// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.Collections;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseSymbols : INameCollection
	{
		private ArrayList collection = new ArrayList();

		public BaseSymbols()
		{
		}

		#region ===== Properties ========================================================

		public int Count
		{
			get { return collection.Count; }
		}
		#endregion

		#region ===== Public methods ====================================================

		public void Add(BaseSymbol symbol)
		{
			collection.Add(symbol);
		}

		public bool ContainsItemNamed(string name)
		{
			return GetSymbolByName(name) != null;
		}

		public BaseSymbol GetSymbolById(int id)
		{
			foreach (BaseSymbol symbol in collection)
			{
				if (symbol.Id == id)
					return symbol;
			}
			return null;
		}

		public BaseSymbol GetSymbolById(string id)
		{
			return GetSymbolById(int.Parse(id));
		}

		public BaseSymbol GetSymbolByName(string name)
		{
			foreach (BaseSymbol symbol in collection)
			{
				if (symbol.Name.ToLower() == name.ToLower())
					return symbol;
			}
			return null;
		}

		public int NewId()
		{
			int id = collection.Count + 1;
			while (GetSymbolById(id) != null)
				id++;
			return id;
		}

		public void Remove(BaseSymbol symbol)
		{
			collection.Remove(symbol);
		}

		public BaseSymbol[] ToArray()
		{
			return (BaseSymbol[])(collection.ToArray(typeof(BaseSymbol)));
		}
		#endregion

		#region ===== Classes and methods required to support enumeration ===============

		public IEnumerator GetEnumerator()
		{
			return new SymbolEnumerator(this);
		}

		private class SymbolEnumerator : IEnumerator
		{
			private int position = -1;
			private BaseSymbols symbols;

			public SymbolEnumerator(BaseSymbols symbols)
			{
				this.symbols = symbols;
			}

			public bool MoveNext()
			{
				if (position < symbols.collection.Count - 1)
				{
					position++;
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				position = -1;
			}

			public object Current
			{
				get
				{
					return symbols.collection[position];
				}
			}
		}
		#endregion
	}
}
