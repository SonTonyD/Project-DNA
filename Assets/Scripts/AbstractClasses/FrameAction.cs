using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DNA
{
    abstract public class FrameAction : MonoBehaviour
    {
        protected bool actionCondition = false;
        protected bool startupCondition = false;
        protected bool activeCondition = false;
        protected bool recoveryCondition = false;
        protected bool resetCondition = false;

        protected abstract void ExecuteStartup();
        protected abstract void ExecuteActive();
        protected abstract void ExecuteRecovery();
        protected abstract void ExecuteReset();
        protected abstract void IncrementFrameCount();

        protected void SetFrameConditions(bool isFrameCountStarted, int frameCount, int startupFrameNumber, int activeFrameNumber, int recoveryFrameNumber)
        {
            startupCondition = !isFrameCountStarted;

            activeCondition = isFrameCountStarted &&
                                frameCount > startupFrameNumber &&
                                frameCount <= startupFrameNumber + activeFrameNumber;

            recoveryCondition = isFrameCountStarted &&
                                frameCount > startupFrameNumber + activeFrameNumber &&
                                frameCount <= startupFrameNumber + activeFrameNumber + recoveryFrameNumber;

            resetCondition = isFrameCountStarted &&
                                frameCount > startupFrameNumber + activeFrameNumber + recoveryFrameNumber;
        }

        protected void ExecuteFrameAction()
        {
            if (actionCondition)
            {
                if (startupCondition)
                {
                    ExecuteStartup();
                }
                else if (activeCondition)
                {
                    ExecuteActive();
                }
                else if (recoveryCondition)
                {
                    ExecuteRecovery();
                }
                else if (resetCondition) {
                    ExecuteReset();
                }
            }

            IncrementFrameCount();
        }
    }
}