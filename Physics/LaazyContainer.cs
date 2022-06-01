using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mag.Physics
{
    public class C8<A, B, C, D, E, F, G, H>
    {
        public A a;//1
        public B b;//2
        public C c;//3
        public D d;//4
        public E e;//5
        public F f;//6
        public G g;//7
        public H h;//8
        public C8(A a, B b, C c, D d, E e, F f, G g, H h)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
            this.g = g;
            this.h = h;
        }

        public override string ToString()
        {
            return base.ToString() +
                $"a:{this.a},b:{this.b},c:{this.c},d:{this.d},e:{this.e},f:{this.f},g:{this.g},h:{this.h}";
        }
    }
    public class C7<A, B, C, D, E, F, G>
    {
        public A a;//1
        public B b;//2
        public C c;//3
        public D d;//4
        public E e;//5
        public F f;//6
        public G g;//7
        public C7(A a, B b, C c, D d, E e, F f, G g)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
            this.g = g;
        }

        public override string ToString()
        {
            return base.ToString() +
                $"a:{this.a},b:{this.b},c:{this.c},d:{this.d},e:{this.e},f:{this.f},g:{this.g}";
        }
    }
    public class C6<A, B, C, D, E, F>
    {
        public A a;//1
        public B b;//2
        public C c;//3
        public D d;//4
        public E e;//5
        public F f;//6
        public C6(A a, B b, C c, D d, E e, F f)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
            this.f = f;
        }

        public override string ToString()
        {
            return base.ToString() +
                $"a:{this.a},b:{this.b},c:{this.c},d:{this.d},e:{this.e},f:{this.f}";
        }
    }
    public class C5<A, B, C, D, E>
    {
        public A a;//1
        public B b;//2
        public C c;//3
        public D d;//4
        public E e;//5
        public C5(A a, B b, C c, D d, E e)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.e = e;
        }

        public override string ToString()
        {
            return base.ToString() +
                $"a:{this.a},b:{this.b},c:{this.c},d:{this.d},e:{this.e}";
        }
    }
    public class C4<A, B, C, D>
    {
        public A a;//1
        public B b;//2
        public C c;//3
        public D d;//4
        public C4(A a, B b, C c, D d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
        }

        public override string ToString()
        {
            return base.ToString() +
                $"a:{this.a},b:{this.b},c:{this.c},d:{this.d}";
        }
    }
    public class C3<A, B, C>
    {
        public A a;//1
        public B b;//2
        public C c;//3
        public C3(A a, B b, C c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public override string ToString()
        {
            return base.ToString() +
                $"a:{this.a},b:{this.b},c:{this.c}";
        }
    }
    public class C2<A, B>
    {
        public A a;//1
        public B b;//2
        public C2(A a, B b)
        {
            this.a = a;
            this.b = b;
        }

        public override string ToString()
        {
            return base.ToString() +
                $"a:{this.a},b:{this.b}";
        }
    }
    public class C1<A>
    {
        public A a;//1
        public C1(A a)
        {
            this.a = a;
        }

        public override string ToString()
        {
            return base.ToString() +
                $"a:{this.a}";
        }
    }
}
