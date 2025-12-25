using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCUtils.SCTween
{
    public class SCTweenSequence
    {
        List<ISCTweenContext> _tweens = new List<ISCTweenContext>();
        Action onFinishCallBack;

        public SCTweenSequence Add(ISCTweenContext context)
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

        public async RwTasks.RwTask RunAsync(CancellationToken token = default)
        {
            foreach (var tween in _tweens)
            {
                await tween.RunAsync(token);
            }
            onFinishCallBack?.Invoke();
        }
    }
}
