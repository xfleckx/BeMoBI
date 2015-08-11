using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class ObjectManager : MonoBehaviour, IEnumerable<Category>
{
    public List<Category> Categories;
    
    public Category GetCategoryBy(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("empty category name");

        var category = Categories.SingleOrDefault((c) => c.name.Equals(name));

        if (category == null)
            throw new ArgumentException(string.Format("Requested Category \"{0}\" not found", name));

        return category;
    }

    public IEnumerator<Category> GetEnumerator()
    {
        return new CategoryEnumerator(Categories);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public class CategoryEnumerator : IEnumerator<Category>
{
    private Stack<Category> stack;

    public CategoryEnumerator(List<Category> categories)
    {
        stack = new Stack<Category>(categories);
    }

    public Category Current
    {
        get
        {
            return stack.Peek();
        }
    }

    object IEnumerator.Current
    {
        get
        {
            return stack.Peek();
        }
    }

    public void Dispose()
    {
        stack.Clear();
    }

    public bool MoveNext()
    {
        if (stack.Count > 0)
        {
            stack.Pop();
            return true;
        }

        return false;
    }

    public void Reset()
    {
        stack = new Stack<Category>(stack);
    }
}