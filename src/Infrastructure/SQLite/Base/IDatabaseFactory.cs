using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Infrastructure.SQLite.Base {
	public interface IDatabaseFactory {
		Database Create();
	}
}
