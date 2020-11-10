using System;
using System.Collections.Generic;
using System.Linq;

namespace lex
{
    public class Lexer
    {
        public Lexer()
        { 
        }
        public bool Nullable(Rexp r) => r switch
        { 
            ZERO _ => false,
            ONE _ => true,
            CHAR _ => false,
            ALT alt => Nullable(alt.r1) || Nullable(alt.r2),
            SEQ seq => Nullable(seq.r1) && Nullable(seq.r2),
            STAR _ => true,
            RECD recd => Nullable(recd.r),
            RANGE _ => false,
            PLUS pl => Nullable(pl.r),
            OPTIONAL _ => true,
            NTIMES nt => nt.n == 0 || Nullable(nt.r),
            UPTO _ => true,
            FROM frm => frm.n == 0 || Nullable(frm.r),
            BETWEEN btw => btw.n == 0 || Nullable(btw.r),
            NOT not => !(Nullable(not.r)),
            CFUN _ => false,
            ALL => false
        };

        public Rexp Derive(char c, Rexp r) => r switch
        {
            ZERO _ => r,
            ONE _ => new ZERO(),
            CHAR ch => Derive(c, new CFUN(ch.c.ToString().ToHashSet())),
            //CHAR ch => (c == ch.c) ? (Rexp)new ONE() : new ZERO(),
            ALT alt => new ALT(Derive(c, alt.r1), Derive(c, alt.r2)),
            SEQ seq => (Nullable(seq.r1)) switch
            {
                true => new ALT(new SEQ(Derive(c, seq.r1), seq.r2), Derive(c, seq.r2)),
                false => new SEQ(Derive(c, seq.r1), seq.r2)
            },
            STAR star => new SEQ(Derive(c, star.r), new STAR(star.r)),
            RANGE rng => Derive(c, new CFUN(rng.s)),
            PLUS plus => new SEQ(Derive(c, plus.r), new STAR(plus.r)),
            RECD recd => Derive(c, recd.r),
            OPTIONAL opt => Derive(c, opt.r),
            NTIMES nt => nt.n == 0 ? (Rexp) new ZERO() : new SEQ(Derive(c, nt.r), new NTIMES(nt.r, nt.n - 1)),
            UPTO upto => upto.m switch
            {
                0 => new ZERO(),
                _ => new SEQ(Derive(c, upto.r), new UPTO(upto.r, upto.m - 1))
            },
            FROM frm => frm.n == 0 ? new SEQ(Derive(c, frm.r), new STAR(r)) : new SEQ(Derive(c, frm.r), new FROM(frm.r, frm.n - 1)),
            BETWEEN btwn => (btwn.r, btwn.n, btwn.m) switch
            {
                (_, _, 0) => new ZERO(),
                (_, _, 1) => Derive(c, btwn.r),
                (_, 0, _) => new SEQ(Derive(c, btwn.r), new BETWEEN(btwn.r, 0, btwn.m - 1)),
                (_, _, _) => new SEQ(Derive(c, btwn.r), new BETWEEN(btwn.r, btwn.n - 1, btwn.m - 1))
            },
            NOT not => new NOT(Derive(c, not.r)),
            //CFUN cf => cf.f(c) ? new ONE() : new ZERO(),
            CFUN cf => cf.f(c) switch
            {
                true => new ONE(),
                false => new ZERO()
            },
            ALL => Derive(c, new CFUN()),
            //_ => throw new System.Exception("GOT UNKNOWN REXP")
        };

/*        public Rexp ders(string s, Rexp r) 
        {
            switch (s.Length)
            {
                case 0:
                    return r;
                default:
                {
                    char c = s[0];
                    s = s.Substring(1);
                    return ders(s, simp(der(c, r)));
                }
            }
        }*/

        public string Flatten(Val v) => v switch
        {
            Empty => "",
            Chr chr => chr.c.ToString(),
            Left left => Flatten(left.v),
            Right right => Flatten(right.v),
            Sequ seq => Flatten(seq.v1) + Flatten(seq.v2),
            Stars stars => string.Join("", stars.vs.Select(Flatten).ToList()),
            Rec rec => Flatten(rec.v)
        };

        public List<(string, string)> Environment(Val v) => v switch
        {
            Empty => null,
            Chr chr => null,
            Left left => Environment(left.v),
            Right right => Environment(right.v),
            Sequ seq => Environment(seq.v1).Concat(Environment(seq.v2)).ToList(),
            Stars stars => stars.vs.SelectMany(Environment).ToList(),
            Rec rec => Environment(rec.v).Prepend((rec.x, Flatten(rec.v))).ToList(),
        };

        public Val Mkeps(Rexp r) => r switch
        {
            ONE _ => new Empty(),
            ALT alt => Nullable(alt.r1) ? (Val)new Left(Mkeps(alt.r1)) : new Right(Mkeps(alt.r2)),
            SEQ seq => new Sequ(Mkeps(seq.r1), Mkeps(seq.r2)),
            STAR _ => new Stars(null),
            RECD recd => new Rec(recd.x, Mkeps(recd.r)),
            PLUS pl => new Stars(Mkeps(pl.r).ToList()),
            OPTIONAL _ => new Stars(null),
            NTIMES nt => new Stars(Enumerable.Repeat(Mkeps(nt.r), nt.n).ToList())
        };

        public Val Inject(Rexp r, char c, Val v) => (r, v) switch
        {
            (STAR st, Sequ se) => se.v2 switch
            {
                Stars vs => new Stars(vs.vs.Prepend(Inject(st.r, c, se.v1)).ToList())
            },

            (SEQ sr, Sequ sv) => new Sequ(Inject(sr.r1, c, sv.v1), sv.v2),

            (SEQ sr, Left lv) => lv.v switch
            {
                Sequ sequ => new Sequ(Inject(sr.r1, c, sequ.v1), sequ.v2)
            },

            (SEQ seq, Right right) => new Sequ(Mkeps(seq.r1), Inject(seq.r2, c, right.v)),

            (ALT alt, Left left) => new Left(Inject(alt.r1, c, left.v)),

            (ALT alt, Right right) => new Right(Inject(alt.r2, c, right.v)),

            (CHAR ch, Empty) => new Chr(ch.c),

            (RECD recd, _) => new Rec(recd.x, Inject(recd.r, c, v)),

            (RANGE, Empty) => new Chr(c),

            (PLUS plus, Sequ sequ) => sequ.v2 switch
            {
                Stars stars => new Stars(stars.vs.Prepend(Inject(plus.r, c, sequ.v1)).ToList())
            },

            // discard appropriate here?
            (OPTIONAL opt, _) => new Stars(Inject(opt.r, c, v).ToList()),

            (NTIMES nt, Sequ seq) => seq.v2 switch
            {
                Stars stars => new Stars(stars.vs.Prepend(Inject(nt.r, c, seq.v1)).ToList())
            }
        };

        // Rectification functions for simplification
        public Val F_ID(Val v) => v;
        public Func<Val, Val> F_RIGHT(Func<Val, Val> f) => (Val v) => new Right(f(v));
        public Func<Val, Val> F_LEFT(Func<Val, Val> f) => (Val v) => new Left(f(v));
        public Func<Val, Val> F_ALT(Func<Val, Val> f1, Func<Val, Val> f2) => (Val v) => v switch
        {
            Right right => new Right(f2(v)),
            Left left => new Left(f1(v))
        };
        public Func<Val, Val> F_SEQ(Func<Val, Val> f1, Func<Val, Val> f2) => (Val v) => v switch
        {
           Sequ sequ => new Sequ(f1(sequ.v1), f2(sequ.v2))
        };

        public Func<Val, Val> F_SEQ_EMPTY1(Func<Val, Val> f1, Func<Val, Val> f2) => (Val v) => new Sequ(f1(new Empty()), f2(v));
        public Func<Val, Val> F_SEQ_EMPTY2(Func<Val, Val> f1, Func<Val, Val> f2) => (Val v) => new Sequ(f1(v), f2(new Empty()));
        public Func<Val, Val> F_RECD(Func<Val, Val> f) => (Val v) => v switch
        {
            Rec rec => new Rec(rec.x, f(rec.v))
        };
        public Func<Val, Val> F_ERROR() => throw new Exception("Error");

        public (Rexp, Func<Val, Val>) Simplify(Rexp r)
        {
            switch (r)
            {
                case ALT alt:
                {
                    var (r1s, f1s) = Simplify(alt.r1);
                    var (r2s, f2s) = Simplify(alt.r2);
                    switch(r1s, r2s)
                    {
                        case (ZERO, _):
                            return (r2s, F_RIGHT(f2s));
                        case (_, ZERO):
                                return (r1s, F_LEFT(f1s));
                        case (_, _):
                            if (r1s == r2s)
                                return (r1s, F_RIGHT(f2s));
                            else
                                return (new ALT(r1s, r2s), F_ALT(f1s, f2s));
                    }
                }
                case SEQ seq:
                {
                    var (r1s, f1s) = Simplify(seq.r1);
                    var (r2s, f2s) = Simplify(seq.r2);
                    switch (r1s, r2s)
                    {
                        case (ZERO, _):
                        case (_, ZERO):
                            return (new ZERO(), F_ERROR());
                        case (ONE, _):
                                return (r2s, F_SEQ_EMPTY1(f1s, f2s));
                        case (_, ONE):
                                return (r1s, F_SEQ_EMPTY2(f1s, f2s));
                        case (_, _):
                            return (new SEQ(r1s, r2s), F_SEQ(f1s, f2s));
                    }
                }
                default:
                    return (r, F_ID);
            }
        }

        public List<(string, string)> RemoveWhitespace (List<(string, string)> lst) => lst.Where(x => x.Item1 != "WS" || x.Item1 != "COM").ToList();

        private Val GetValues (Rexp r, string s)
        {
            if (s.Length == 0)
            {
                if (Nullable(r))
                    return Mkeps(r);
                else
                    throw new Exception("Unable to Lex");
            }
            else
            {
                char c = s[0];
                s = s.Substring(1);
                var (r_simp, f_simp) = Simplify(Derive(c, r));
                return Inject(r, c, f_simp(GetValues(r_simp, s)));
            }
        }

        public List<(string, string)> Lex (Rexp r, string s) => Environment(GetValues(r, s));
        
/*        public bool Match(Rexp r, string s)
        {
            return nullable(ders(s, r));
        }*/

        public int Size(Rexp r) => r switch
        {
            ZERO _ => 1,
            ONE _ => 1,
            CHAR _ => 1,
            ALT alt => 1 + Size(alt.r1) + Size(alt.r2),
            SEQ seq => 1 + Size(seq.r1) + Size(seq.r2),
            STAR star => 1 + Size(star.r)
        };
    }
}
