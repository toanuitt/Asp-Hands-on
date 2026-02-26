using AspNetHandons.Entities;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace AspNetHandons.ExternalApis
{
    public interface IJsonTodo
    {
        [Get("/todos/{id}")]
        Task<Todo> GetTodo(int id);
    }
}

