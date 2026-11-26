using Hikyaku;
using FIWARE.Orion.Client;
using FIWARE.Orion.Client.Models;
using MapZilla;
using Microsoft.Extensions.Logging;
using Wise.Tasks.Core.Notifications;

namespace Wisetown.ContextBroker.Worker.Handlers;

public class TasksNotificationsHandler :
  INotificationHandler<Wise.Tasks.Core.Notifications.TaskCreated>,
  INotificationHandler<Wise.Tasks.Core.Notifications.TaskChanged>,
  INotificationHandler<Wise.Tasks.Core.Notifications.TaskDeleted>,
  INotificationHandler<Wise.Tasks.Core.Notifications.TaskArchived>
{
  private readonly OrionClient _orionClient;
  private readonly IMapper _mapper;
  private readonly ILogger<TasksNotificationsHandler> _logger;

  public TasksNotificationsHandler(FIWARE.Orion.Client.OrionClient orionClient, IMapper mapper, ILogger<TasksNotificationsHandler> logger)
  {
    _orionClient = orionClient;
    _mapper = mapper;
    _logger = logger;
  }

  public async Task Handle(TaskCreated notification, CancellationToken cancellationToken)
  {
    var model = _mapper.Map<FIWARE.SmartDataModels.IssueTracking.IssueReporting>(notification.Task);
    var celement = model.ToContextElement(i => i.Type, i => i.Id);
    
    var response = await _orionClient.AddEntityAsync(celement);

    if (response?.StatusCode?.Code == "200")
      _logger.LogInformation($"{notification.Task.Group.TaskType} {notification.Task.Id} added to context");
    else
      _logger.LogError($"Error adding {notification.Task.Group.TaskType} {notification.Task.Id} to context, {response?.StatusCode?.ReasonPhrase ?? ""}");
  }

  public async Task Handle(TaskChanged notification, CancellationToken cancellationToken)
  {
    var model = _mapper.Map<FIWARE.SmartDataModels.IssueTracking.IssueReporting>(notification.New);

    var exists = await _orionClient.GetEntityAsync(model.Id);

    ContextResponse response = null;
    
    var celement = model.ToContextElement(i => i.Type, i => i.Id);
    if (exists.StatusCode.Code == "404")
      response = await _orionClient.AddEntityAsync(celement);
    else
      response = await _orionClient.UpdateEntityAsync(celement.Id, celement);
    
    if (response?.StatusCode?.Code == "200")
      _logger.LogInformation($"{notification.New.Group.TaskType} {notification.New.Id} added to context");
    else
      _logger.LogError($"Error adding {notification.New.Group.TaskType} {notification.New.Id} to context, {response?.StatusCode?.ReasonPhrase ?? ""}");
  }

  public async Task Handle(TaskDeleted notification, CancellationToken cancellationToken)
  {
    var model = _mapper.Map<FIWARE.SmartDataModels.IssueTracking.IssueReporting>(notification.Task);
    var celement = model.ToContextElement(i => i.Type, i => i.Id);
    var response = await _orionClient.DeleteEntityAsync(celement.Id);
    
    if (response?.StatusCode?.Code == "200")
      _logger.LogInformation($"{notification.Task.Group.TaskType} {notification.Task.Id} added to context");
    else
      _logger.LogError($"Error adding {notification.Task.Group.TaskType} {notification.Task.Id} to context, {response?.StatusCode?.ReasonPhrase ?? ""}");
  }

  public async Task Handle(TaskArchived notification, CancellationToken cancellationToken)
  {
    var model = _mapper.Map<FIWARE.SmartDataModels.IssueTracking.IssueReporting>(notification.Task);
    var celement = model.ToContextElement(i => i.Type, i => i.Id);
    var response = await _orionClient.DeleteEntityAsync(celement.Id);
    
    if (response?.StatusCode?.Code == "200")
      _logger.LogInformation($"{notification.Task.Group.TaskType} {notification.Task.Id} added to context");
    else
      _logger.LogError($"Error adding {notification.Task.Group.TaskType} {notification.Task.Id} to context, {response?.StatusCode?.ReasonPhrase ?? ""}");
  }
}
