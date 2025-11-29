using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> heap = new List<(T, float)>();
    private Dictionary<T, int> indexMap = new Dictionary<T, int>(); // item ¡ú index

    public int Count => heap.Count;

    public bool Contains(T item)
        => indexMap.ContainsKey(item);

    public void Enqueue(T item, float priority)
    {
        heap.Add((item, priority));
        int index = heap.Count - 1;
        indexMap[item] = index;
        HeapifyUp(index);
    }

    public T Dequeue()
    {
        if (heap.Count == 0) return default;

        var root = heap[0].item;

        // Move last to root
        int last = heap.Count - 1;
        heap[0] = heap[last];
        indexMap[heap[0].item] = 0;

        heap.RemoveAt(last);
        indexMap.Remove(root);

        if (heap.Count > 0)
            HeapifyDown(0);

        return root;
    }

    public void UpdatePriority(T item, float newPriority)
    {
        if (!indexMap.TryGetValue(item, out int index))
            return; // If not found, ignore

        float oldPriority = heap[index].priority;
        heap[index] = (item, newPriority);

        if (newPriority < oldPriority)
            HeapifyUp(index);
        else
            HeapifyDown(index);
    }

    public List<T> ToList()
    {
        List<T> result = new List<T>();
        foreach (var (it, _) in heap) result.Add(it);
        return result;
    }

    private void HeapifyUp(int index)
    {
        while (index > 0)
        {
            int parent = (index - 1) / 2;

            if (heap[index].priority >= heap[parent].priority) break;

            Swap(index, parent);
            index = parent;
        }
    }

    private void HeapifyDown(int index)
    {
        int last = heap.Count - 1;

        while (true)
        {
            int left = index * 2 + 1;
            int right = index * 2 + 2;
            int smallest = index;

            if (left <= last && heap[left].priority < heap[smallest].priority)
                smallest = left;
            if (right <= last && heap[right].priority < heap[smallest].priority)
                smallest = right;

            if (smallest == index) break;

            Swap(index, smallest);
            index = smallest;
        }
    }

    private void Swap(int i, int j)
    {
        var temp = heap[i];
        heap[i] = heap[j];
        heap[j] = temp;

        indexMap[heap[i].item] = i;
        indexMap[heap[j].item] = j;
    }
}
