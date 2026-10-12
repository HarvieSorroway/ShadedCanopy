using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Deepseek生成的 低版本net自己没有

namespace ShadedCanopy.Objects.SCChandelierFlower.Mesh3D
{

    /// <summary>
    /// 优先队列类（支持泛型元素类型和泛型优先级类型）
    /// </summary>
    /// <typeparam name="TElement">元素类型</typeparam>
    /// <typeparam name="TPriority">优先级类型（必须实现IComparable接口）</typeparam>
    public class PriorityQueue<TElement, TPriority>: IEnumerable<TElement> where TPriority : IComparable<TPriority>
    {
        private struct QueueItem
        {
            public TElement Element;
            public TPriority Priority;

            public QueueItem(TElement element, TPriority priority)
            {
                this.Element = element;
                this.Priority = priority;
            }
        }

        private List<QueueItem> heap;
        private bool isMaxHeap;
        private Comparison<TPriority> priorityComparison;

        /// <summary>
        /// 初始化优先队列（默认为小根堆）
        /// </summary>
        public PriorityQueue() : this(false)
        {
        }

        /// <summary>
        /// 初始化优先队列
        /// </summary>
        /// <param name="isMaxHeap">true:大根堆（优先级高的先出队），false:小根堆（优先级小的先出队）</param>
        public PriorityQueue(bool isMaxHeap)
        {
            this.heap = new List<QueueItem>();
            this.isMaxHeap = isMaxHeap;

            // 根据堆类型确定优先级比较规则
            if (isMaxHeap)
            {
                this.priorityComparison = (p1, p2) => p2.CompareTo(p1);
            }
            else
            {
                this.priorityComparison = (p1, p2) => p1.CompareTo(p2);
            }
        }

        /// <summary>
        /// 初始化优先队列（指定初始容量）
        /// </summary>
        /// <param name="capacity">初始容量</param>
        /// <param name="isMaxHeap">true:大根堆，false:小根堆</param>
        public PriorityQueue(int capacity, bool isMaxHeap)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException("capacity", "容量不能为负数");

            this.heap = new List<QueueItem>(capacity);
            this.isMaxHeap = isMaxHeap;

            if (isMaxHeap)
            {
                this.priorityComparison = (p1, p2) => p2.CompareTo(p1);
            }
            else
            {
                this.priorityComparison = (p1, p2) => p1.CompareTo(p2);
            }
        }

        /// <summary>
        /// 获取队列中的元素数量
        /// </summary>
        public int Count
        {
            get { return heap.Count; }
        }

        /// <summary>
        /// 判断队列是否为空
        /// </summary>
        public bool IsEmpty
        {
            get { return heap.Count == 0; }
        }

        /// <summary>
        /// 获取堆类型
        /// </summary>
        public bool IsMaxHeap
        {
            get { return isMaxHeap; }
        }

        /// <summary>
        /// 添加元素到优先队列
        /// </summary>
        /// <param name="element">要添加的元素</param>
        /// <param name="priority">优先级</param>
        public void Enqueue(TElement element, TPriority priority)
        {
            QueueItem newItem = new QueueItem(element, priority);
            heap.Add(newItem);
            int currentIndex = heap.Count - 1;

            // 上浮操作
            while (currentIndex > 0)
            {
                int parentIndex = (currentIndex - 1) / 2;

                if (ComparePriority(heap[currentIndex].Priority, heap[parentIndex].Priority) >= 0)
                    break;

                Swap(currentIndex, parentIndex);
                currentIndex = parentIndex;
            }
        }

        /// <summary>
        /// 移除并返回优先级最高的元素
        /// </summary>
        /// <returns>优先级最高的元素</returns>
        /// <exception cref="InvalidOperationException">队列为空时抛出异常</exception>
        public TElement Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("优先队列为空");

            TElement result = heap[0].Element;
            int lastIndex = heap.Count - 1;

            // 将最后一个元素移到根节点
            heap[0] = heap[lastIndex];
            heap.RemoveAt(lastIndex);

            if (heap.Count > 0)
            {
                // 下沉操作
                int currentIndex = 0;
                while (true)
                {
                    int leftChild = currentIndex * 2 + 1;
                    int rightChild = currentIndex * 2 + 2;
                    int smallest = currentIndex;

                    if (leftChild < heap.Count &&
                        ComparePriority(heap[leftChild].Priority, heap[smallest].Priority) < 0)
                    {
                        smallest = leftChild;
                    }

                    if (rightChild < heap.Count &&
                        ComparePriority(heap[rightChild].Priority, heap[smallest].Priority) < 0)
                    {
                        smallest = rightChild;
                    }

                    if (smallest == currentIndex)
                        break;

                    Swap(currentIndex, smallest);
                    currentIndex = smallest;
                }
            }

            return result;
        }

        /// <summary>
        /// 查看优先级最高的元素（不移除）
        /// </summary>
        /// <returns>优先级最高的元素</returns>
        /// <exception cref="InvalidOperationException">队列为空时抛出异常</exception>
        public TElement Peek()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("优先队列为空");

            return heap[0].Element;
        }

        /// <summary>
        /// 查看优先级最高的元素及其优先级（不移除）
        /// </summary>
        /// <param name="element">输出元素</param>
        /// <param name="priority">输出优先级</param>
        /// <returns>是否成功获取</returns>
        public bool TryPeek(out TElement element, out TPriority priority)
        {
            if (heap.Count == 0)
            {
                element = default(TElement);
                priority = default(TPriority);
                return false;
            }

            element = heap[0].Element;
            priority = heap[0].Priority;
            return true;
        }

        /// <summary>
        /// 清空优先队列
        /// </summary>
        public void Clear()
        {
            heap.Clear();
        }

        /// <summary>
        /// 比较两个优先级（根据堆类型返回比较结果）
        /// </summary>
        private int ComparePriority(TPriority p1, TPriority p2)
        {
            return priorityComparison(p1, p2);
        }

        /// <summary>
        /// 交换两个位置的元素
        /// </summary>
        private void Swap(int i, int j)
        {
            QueueItem temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in heap)
            {
                yield return item.Element;
            }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return heap.Select(item => item.Element).GetEnumerator();
        }
    }
}