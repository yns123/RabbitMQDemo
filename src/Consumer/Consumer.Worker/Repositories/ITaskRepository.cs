using Consumer.Worker.Domain;

namespace Consumer.Worker.Repositories;

public interface ITaskRepository
{
    void Add(TaskItem task);
}
