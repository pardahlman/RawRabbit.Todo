using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RawRabbit.Todo.Shared
{
	public class Todo
	{
		public int Id { get; set; }
		public string Owner { get; set; }
		public string Task { get; set; }
		public bool Completed { get; set; }
	}
}
