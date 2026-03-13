// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;

public class ActionQueue
{
	private int actionsInQueue;
	private int front;
	private MemberPageActionId[] queue;
	private int totalActions;

	public ActionQueue(int length)
	{
		queue = new MemberPageActionId[length];
		actionsInQueue = 0;
		front = -1;
	}

	public void Add(MemberPageActionId actionId)
	{
		try
		{
			if (actionsInQueue < queue.Length)
				actionsInQueue++;

			totalActions++;
			front++;
			
			if (front == queue.Length)
				front = 0;

			queue[front] = actionId;

			Debug.WriteLine("=====");
			Debug.WriteLine(Dump());
		}
		catch (Exception ex)
		{
			Utility.ReportException("ActionQueue.Add", ex);
		}
	}

	public string Dump()
	{
		string content = string.Empty;

		try
		{
			int index = front;
			int count = actionsInQueue;
            int dumpCount = 0;

			while (count > 0)
			{
                if (content.Length > 0)
					content += "\n";
				content += String.Format("{0}: {1}", index, (MemberPageActionId)queue[index]);

				index--;

				if (index < 0)
					index = queue.Length - 1;

				count--;

                // Only dump the last 10 actions.
                dumpCount++;
                if (dumpCount >= 10)
                    break;
			}
		}
		catch (Exception ex)
		{
			content = "Exception occurred while dumping action queue: " + ex.Message;
		}
		
		return string.Format("{0}", content);
	}
}
