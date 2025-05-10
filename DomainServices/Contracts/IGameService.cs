using DomainModels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainServices.Contracts;

public interface IGameService
{
    public Task<(User user, string error)> CreateGameAsync(string firstUserId, string secondUserId);

}
