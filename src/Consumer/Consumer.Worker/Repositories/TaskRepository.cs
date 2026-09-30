using Consumer.Worker.Data;
using Consumer.Worker.Domain;

namespace Consumer.Worker.Repositories;

public class TaskRepository(ConsumerDbContext context) : ITaskRepository
{
    public void Add(TaskItem task) => context.Tasks.Add(task);
}
