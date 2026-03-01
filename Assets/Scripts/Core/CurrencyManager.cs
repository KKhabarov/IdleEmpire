using System;
using System.Threading;
using UnityEngine;

namespace IdleEmpire.Core
{
    /// <summary>
    /// Manages the player's money. All monetary operations are thread-safe.
    /// Fires <see cref="OnMoneyChanged"/> whenever the balance is updated so UI can react.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        #region Events

        /// <summary>
        /// Invoked after every balance change. Passes the new total as a <c>double</c>.
        /// Subscribe from UI scripts to refresh displays.
        /// </summary>
        public event Action<double> OnMoneyChanged;

        #endregion

        #region Private Fields

        // Backing store for the money value. Access is guarded by _lock.
        private double _money;

        // Lock object used to make read/write operations thread-safe.
        private readonly object _lock = new object();

        // The managed thread ID of the Unity main thread, captured in Awake().
        private int _mainThreadId;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            // Capture the main thread ID once during initialization.
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        private void Update()
        {
            // Drain any callbacks queued from background threads back to the main thread.
            UnityMainThreadDispatcher.Execute();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Adds <paramref name="amount"/> to the current balance.
        /// </summary>
        /// <param name="amount">Positive amount to add.</param>
        public void AddMoney(double amount)
        {
            if (amount <= 0) return;

            double newBalance;
            lock (_lock)
            {
                _money += amount;
                newBalance = _money;
            }

            NotifyChanged(newBalance);
        }

        /// <summary>
        /// Deducts <paramref name="amount"/> from the current balance.
        /// </summary>
        /// <param name="amount">Positive amount to deduct.</param>
        /// <returns><c>true</c> if the deduction was successful; <c>false</c> if insufficient funds.</returns>
        public bool SpendMoney(double amount)
        {
            if (amount <= 0) return false;

            double newBalance;
            lock (_lock)
            {
                if (_money < amount) return false;
                _money -= amount;
                newBalance = _money;
            }

            NotifyChanged(newBalance);
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the player currently has at least <paramref name="amount"/> money.
        /// </summary>
        /// <param name="amount">Amount to check against.</param>
        public bool CanAfford(double amount)
        {
            lock (_lock)
            {
                return _money >= amount;
            }
        }

        /// <summary>Returns the current money balance.</summary>
        public double GetMoney()
        {
            lock (_lock)
            {
                return _money;
            }
        }

        /// <summary>
        /// Directly sets the balance to <paramref name="value"/>. 
        /// Used during save/load and prestige resets — not for normal gameplay.
        /// </summary>
        /// <param name="value">New balance value (non-negative).</param>
        public void SetMoney(double value)
        {
            double newBalance;
            lock (_lock)
            {
                _money = Math.Max(0, value);
                newBalance = _money;
            }

            NotifyChanged(newBalance);
        }

        #endregion

        #region Helpers

        private void NotifyChanged(double newBalance)
        {
            // Marshal back to the main thread if called from a background thread.
            if (System.Threading.Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                OnMoneyChanged?.Invoke(newBalance);
            }
            else
            {
                // Queue for next Unity frame on the main thread via a helper dispatcher.
                UnityMainThreadDispatcher.Enqueue(() => OnMoneyChanged?.Invoke(newBalance));
            }
        }

        #endregion
    }

    /// <summary>
    /// Minimal main-thread dispatcher used by <see cref="CurrencyManager"/> to marshal
    /// callbacks from background threads back to Unity's main thread.
    /// </summary>
    internal static class UnityMainThreadDispatcher
    {
        private static readonly System.Collections.Generic.Queue<Action> _queue
            = new System.Collections.Generic.Queue<Action>();
        private static readonly object _queueLock = new object();

        /// <summary>Enqueues <paramref name="action"/> to run on the next main-thread update.</summary>
        internal static void Enqueue(Action action)
        {
            lock (_queueLock)
            {
                _queue.Enqueue(action);
            }
        }

        /// <summary>
        /// Processes all pending actions. Call this from a MonoBehaviour.Update on the main thread.
        /// </summary>
        internal static void Execute()
        {
            while (true)
            {
                Action action;
                lock (_queueLock)
                {
                    if (_queue.Count == 0) break;
                    action = _queue.Dequeue();
                }
                action?.Invoke();
            }
        }
    }
}
