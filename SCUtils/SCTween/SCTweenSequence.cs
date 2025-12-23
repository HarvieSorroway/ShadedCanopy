using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils.SCTween
{
    public class SCTweenSequence
    {
        List<SCTweenContextBase> _tweens = new List<SCTweenContextBase>();
        Action onFinishCallBack;

        public SCTweenSequence Add(SCTweenContextBase context)
        {
            _tweens.Add(context);
            return this;
        }

        public SCTweenSequence OnFinish(Action finishCallBack)
        {
            if(_tweens.Count > 0)
            {
                onFinishCallBack = finishCallBack;
            }
            return this;
        }

        public async RwTasks.RwTask RunAsync()
        {
            foreach (var tween in _tweens)
            {
                await tween.RunAsync();
            }
            onFinishCallBack?.Invoke();
        }
    }
}
