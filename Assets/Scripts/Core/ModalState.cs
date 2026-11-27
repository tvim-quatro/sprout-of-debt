using UnityEngine;

namespace SproutOfDebt.Core
{
    public class ModalState : IModalState
    {
        private int openCount;
        private int lastClosedFrame = -1;

        public bool IsOpen => openCount > 0 || lastClosedFrame == Time.frameCount;

        public void Opened()
        {
            openCount++;
        }

        public void Closed()
        {
            if (openCount <= 0)
            {
                return;
            }
            openCount--;
            lastClosedFrame = Time.frameCount;
        }
    }
}
