using UnityEngine;
using System.Collections;
using Utils;


public class BaseServer
{
	private static int s_instanceId;
	private string m_requestGroupId;

	protected BaseServer(Core.BaseBehaviour owner)
	{
		if (owner != null)
		{
			owner.RegisterServer(this);
			m_requestGroupId = ToString() + (s_instanceId++);
		}
	}

	protected string GetRequestGroup()
	{
		return m_requestGroupId;
	}

	public void CancelRequests()
	{
		Kanga.Server.Instance.CancelGroupRequest(m_requestGroupId);
	}
}

