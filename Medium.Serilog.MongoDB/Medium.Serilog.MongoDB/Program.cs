using Medium.Serilog.MongoDB;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var logger = new LoggerClass(app.Configuration);
logger.ExecutLogger(TypeLog.Audit,
                    TypeLevel.Information,
                    userNameLog: "raidy",
                    message: "Iniciando aplicação no program.cs");

app.MapGet("/", () => "Hello World!");

app.Run();