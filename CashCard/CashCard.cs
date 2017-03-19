using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kostas.CC
{
    public class CashCard : ICashCard
    {
        private TimeSpan pinValidationTimeSpan;

        private Object lockObj = new Object();

        private decimal balance; // NOTE: money is better represented with decimal, rather than float or double
        public decimal Balance
        {
            get
            {
                lock (lockObj)
                {
                    return balance;
                }
            }
        }
        
        public CashCard()
        {
            pinValidationTimeSpan = new TimeSpan(0, 0, 10);
        }

        /// <summary>
        /// PIN Validation. Outside the scope of this excercise, could use external pin validator, method implementation here, a pin validator passed in the CashCard constructor or any other solution.
        /// </summary>
        /// <param name="pin">The PIN number, usually a four digit integer</param>
        /// <returns>Operation Success (true) or Failure (false) expressed as a boolean</returns>
        private async Task<bool> ValidatePin(int pin)
        {
            return pin.Equals(1234); // Implementation ommitted for the purpose of this excercise
        }

        /// <summary>
        /// Asynchronously withdraw an amount from the CashCard
        /// </summary>
        /// <param name="pin">PIN number</param>
        /// <param name="amount">Amount to withdraw</param>
        /// <returns>Operation Success (true) or Failure (false) expressed as a boolean</returns>
        public async Task<bool> Withdraw(int pin, decimal amount)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(pinValidationTimeSpan);
            bool pinValid = await ValidatePin(pin);
            cts.Dispose();
            if (!pinValid) return false;

            // NOTE: unfortunately Interlocked does not support decimals for decrementing, so we have to use lock()
            lock (lockObj)
            {
                if (amount > balance)
                {
                    throw new TaskCanceledException("Amount requested exceeds balance");
                }
                balance -= amount;
                return true;
            }
        }

        /// <summary>
        /// Asynchronously TopUp the CashCard with an amount
        /// </summary>
        /// <param name="pin">PIN number</param>
        /// <param name="amount">Amount to top up with</param>
        /// <returns>Operation Success (true) or Failure (false) expressed as a boolean</returns>
        public async Task<bool> TopUp(int pin, decimal amount)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(pinValidationTimeSpan);
            bool pinValid = await ValidatePin(pin);
            cts.Dispose();
            if (!pinValid) return false;

            // NOTE: unfortunately Interlocked does not support decimals for incrementing, so we have to use lock()
            lock (lockObj)
            {
                balance += amount;
                return true;
            }
        }
    }
}
