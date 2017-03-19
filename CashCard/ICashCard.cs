using System;
using System.Threading.Tasks;

namespace Kostas.CC
{
    public interface ICashCard
    {
        Task<bool> Withdraw(int pin, decimal amount);
        Task<bool> TopUp(int pin, decimal amount);
    }
}
