using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Kostas.CC;

namespace CreditCardTests
{
    [TestFixture]
    public class CashCardTests
    {
        [Test]
        public async Task Test_TopUp_Basic()
        {
            CashCard cashCard = new CashCard();
            var initialBalance = cashCard.Balance;
            var topUpAmount = 1000M;
            var topUpSuccess = await cashCard.TopUp(1234, topUpAmount);
            Assert.IsTrue(topUpSuccess);
            Assert.AreEqual(initialBalance + topUpAmount, cashCard.Balance);
        }

        [Test]
        public async Task Test_TopUp_FailOnBalanceOverflow()
        {
            CashCard cashCard = new CashCard();
            // add an initial balance of 1,000
            var addInitialBalanceSuccess = await cashCard.TopUp(1234, 1000M);
            Assert.IsTrue(addInitialBalanceSuccess);
            var initialBalance = cashCard.Balance;
            // now try to top up with an amount equal to the max decimal value - will throw an OverflowException
            var topUpAmount = Decimal.MaxValue;
            Assert.That(async () => await cashCard.TopUp(1234, topUpAmount),
                Throws.TypeOf<OverflowException>());
            // confirm balance is unchanged
            Assert.AreEqual(initialBalance, cashCard.Balance);
        }

        [Test]
        public async Task Test_TopUp_FailOnPin()
        {
            CashCard cashCard = new CashCard();
            var initialBalance = cashCard.Balance;
            // topup by 1,000
            var topupSuccess = await cashCard.TopUp(4321, 1000M);
            Assert.IsFalse(topupSuccess);
            Assert.AreEqual(initialBalance, cashCard.Balance);

        }



        [Test]
        public async Task Test_Withdraw_Basic()
        {
            CashCard cashCard = new CashCard();
            // add an initial balance of 10,000
            var addInitialBalanceSuccess = await cashCard.TopUp(1234, 10000M);
            Assert.IsTrue(addInitialBalanceSuccess);

            // try to withdraw 1,000
            var initialBalance = cashCard.Balance;
            var withdrawAmount = 1000M;
            var withdrawSuccess = await cashCard.Withdraw(1234, withdrawAmount);
            Assert.IsTrue(withdrawSuccess);
            Assert.AreEqual(initialBalance - withdrawAmount, cashCard.Balance);
        }

        [Test]
        public async Task Test_Withdraw_FailOnInsufficientBalance()
        {
            CashCard cashCard = new CashCard();
            // add an initial balance of 500
            var addInitialBalanceSuccess = await cashCard.TopUp(1234, 500M);
            Assert.IsTrue(addInitialBalanceSuccess);

            // try to withdraw 1,000
            var initialBalance = cashCard.Balance;
            var withdrawAmount = 1000M;
            Assert.That(async () => await cashCard.Withdraw(1234, withdrawAmount),
                Throws.TypeOf<TaskCanceledException>());
            Assert.AreEqual(initialBalance, cashCard.Balance);
        }

        [Test]
        public async Task Test_Withdraw_FailOnPin()
        {
            CashCard cashCard = new CashCard();
            // add an initial balance of 10,000
            var addInitialBalanceSuccess = await cashCard.TopUp(1234, 10000M);
            Assert.IsTrue(addInitialBalanceSuccess);

            // try to withdraw 1,000 but use a wrong PIN
            var initialBalance = cashCard.Balance;
            var withdrawAmount = 1000M;
            var withdrawSuccess = await cashCard.Withdraw(4321, withdrawAmount);
            Assert.IsFalse(withdrawSuccess);
            Assert.AreEqual(initialBalance, cashCard.Balance);
        }
        
        [Test]
        public async Task Test_Withdraw_FromMultipleSources()
        {
            CashCard cashCard = new CashCard();
            var addInitialBalanceSuccess = await cashCard.TopUp(1234, 10000M);
            var initialBalance = cashCard.Balance;

            var task1 = cashCard.Withdraw(1234, 100M);
            var task2 = cashCard.Withdraw(1234, 500M);
            var task3 = cashCard.Withdraw(1234, 1000M);
            var task4 = cashCard.Withdraw(1234, 10M);
            var task5 = cashCard.Withdraw(1234, 50M);
            var results = await Task.WhenAll(task1, task2, task3, task4, task5);
            Assert.IsTrue(results[0]);
            Assert.IsTrue(results[1]);
            Assert.IsTrue(results[2]);
            Assert.IsTrue(results[3]);
            Assert.IsTrue(results[4]);
            Assert.AreEqual(initialBalance - 100M - 500M - 1000M - 10M - 50M, cashCard.Balance);
        }

        [Test]
        public async Task Test_Withdraw_FromMultipleSourcesOneWrongPin()
        {
            CashCard cashCard = new CashCard();
            var addInitialBalanceSuccess = await cashCard.TopUp(1234, 10000M);
            var initialBalance = cashCard.Balance;

            var task1 = cashCard.Withdraw(1234, 100M);
            var task2 = cashCard.Withdraw(1234, 500M);
            var task3 = cashCard.Withdraw(4321, 1000M);
            var task4 = cashCard.Withdraw(1234, 10M);
            var task5 = cashCard.Withdraw(1234, 50M);
            var results = await Task.WhenAll(task1, task2, task3, task4, task5);
            Assert.IsTrue(results[0]);
            Assert.IsTrue(results[1]);
            Assert.IsFalse(results[2]);
            Assert.IsTrue(results[3]);
            Assert.IsTrue(results[4]);
            Assert.AreEqual(initialBalance - 100M - 500M - 10M - 50M, cashCard.Balance);
        }
    }
}
