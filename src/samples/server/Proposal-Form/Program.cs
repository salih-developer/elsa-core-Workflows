using Elsa;
using Elsa.Models;
using Elsa.Persistence.EntityFramework.SqlServer;
using Elsa.Retention.Extensions;
using Elsa.Retention.Filters;
using Elsa.Retention.Specifications;
using Elsa.WorkflowTesting.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using Proposal.Form.Database;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
ConfigurationManager configuration = builder.Configuration;
var elsaSection = configuration.GetSection("Elsa");
var startups = new[]
           {
                typeof(Elsa.Activities.Console.Startup),
                typeof(Elsa.Activities.Http.Startup),
                typeof(Elsa.Activities.AzureServiceBus.Startup),
                typeof(Elsa.Activities.Conductor.Startup),
                typeof(Elsa.Activities.UserTask.Startup),
                typeof(Elsa.Activities.Temporal.Quartz.Startup),
                typeof(Elsa.Activities.Temporal.Hangfire.Startup),
                typeof(Elsa.Activities.Email.Startup),
                typeof(Elsa.Activities.Telnyx.Startup),
                typeof(Elsa.Activities.File.Startup),
                typeof(Elsa.Activities.RabbitMq.Startup),
                typeof(Elsa.Activities.Sql.Startup),
                typeof(Elsa.Activities.Mqtt.Startup),
                typeof(Elsa.Persistence.EntityFramework.Sqlite.Startup),
                typeof(Elsa.Persistence.MongoDb.Startup),
                typeof(Elsa.Persistence.YesSql.SqlServerStartup),
                typeof(Elsa.Server.Hangfire.Startup),
                typeof(Elsa.Scripting.JavaScript.Startup),
                typeof(Elsa.Activities.Webhooks.Startup),
                typeof(Elsa.Webhooks.Persistence.EntityFramework.SqlServer.Startup),
                typeof(Elsa.Webhooks.Persistence.MongoDb.Startup),
                typeof(Elsa.Webhooks.Persistence.YesSql.SqlServerStartup),
                typeof(Elsa.WorkflowSettings.Persistence.EntityFramework.SqlServer.Startup),
                typeof(Elsa.WorkflowSettings.Persistence.MongoDb.Startup),
                typeof(Elsa.WorkflowSettings.Persistence.YesSql.SqlServerStartup),
            };

services
    .AddElsa(elsa => elsa
    .AddHttpActivities(elsaSection.GetSection("Server").Bind)
    .AddEmailActivities(elsaSection.GetSection("Smtp").Bind)
    .AddConsoleActivities()
    .AddJavaScriptActivities()
    .AddActivitiesFrom<Proposal.Form.Startup>()
    .AddWorkflowsFrom<Proposal.Form.Startup>()
    .AddFeatures(startups, configuration)
    .ConfigureWorkflowChannels(options => elsaSection.GetSection("WorkflowChannels").Bind(options))

    // Optionally opt-out of indexing workflows stored in the database.
    // These will be indexed when published/unpublished/deleted, so no need to do it during startup.
    // Unless you have existing workflow definitions in the DB for which no triggers have yet been created.
    //.ExcludeWorkflowProviderFromStartupIndexing<DatabaseWorkflowProvider>()

    // For distributed hosting, configure Rebus with a real message broker such as RabbitMQ or Azure Service Bus.
    //.UseRabbitMq(Configuration.GetConnectionString("RabbitMq"))

    // When testing a distributed on your local machine, make sure each instance has a unique "container" name.
    // This name is used to create unique input queues for pub/sub messaging where the competing consumer pattern is undesirable in order to deliver a message to each subscriber.
    //.WithContainerName(Configuration.GetValue<string>("ContainerName") ?? System.Environment.MachineName)
    )
    .AddRetentionServices(options =>
    {
        // Bind options from configuration.
        elsaSection.GetSection("Retention").Bind(options);


        // Configure a custom specification filter (server side) pipeline that deletes cancelled, faulted and completed workflows.
        options.ConfigureSpecificationFilter = filter => filter.AddAndSpecification(
            new WorkflowStatusFilterSpecification(WorkflowStatus.Cancelled, WorkflowStatus.Faulted, WorkflowStatus.Finished))
        ;

        // Configure a custom filter pipeline that deletes completed AND faulted workflows.
        options.ConfigurePipeline = pipeline => pipeline
            .AddFilter(new WorkflowStatusFilter(WorkflowStatus.Cancelled, WorkflowStatus.Faulted, WorkflowStatus.Finished))
            // Could add additional filters. For example, if there's a way to know that some workflow is a child workflow, maybe don't delete the parent.
            ;
    });



builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddDbContext<ProposalDbContext>(item => item.UseSqlite(elsaSection.GetConnectionString("Sqlite"), x => {}));

builder.Services
             .AddNotificationHandlersFrom<Proposal.Form.Startup>()
             .AddElsaApiEndpoints();
builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("Content-Disposition")));

// Workflow Testing
builder.Services.AddWorkflowTestingServices();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    //app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dojo App"));

app.UseStaticFiles();
//app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(cors => cors
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetIsOriginAllowed(_ => true)
                   .AllowCredentials());
app.UseElsaFeatures();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
