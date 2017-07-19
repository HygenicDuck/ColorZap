using UnityEngine;
using System.Collections;
using Core;

namespace UI
{
	public class TableViewCell : BaseBehaviour
	{
		private int m_id;
		private bool m_recycleCell = true;

		public int ID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}

		public bool RecycleCell
		{
			get
			{
				return m_recycleCell;
			}
			set
			{
				m_recycleCell = value;
			}
		}

		public void Reset()
		{ 
			m_id = TableView.INVALID_INDEX; 
		}
	}
}
