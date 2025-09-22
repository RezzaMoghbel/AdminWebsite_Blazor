using System.Data;

namespace Mars.Admin.Data.Infrastructure;

public interface ISqlConnectionFactory { IDbConnection Create(); }
