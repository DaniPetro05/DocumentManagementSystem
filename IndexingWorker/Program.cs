using IndexingWorker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<IndexingWorkerService>();

var host = builder.Build();
host.Run();
