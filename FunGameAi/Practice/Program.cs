using System;
using System.Linq;

class P
{
static void Main2()
{
var n=Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
var (q,w)=(n[0]-n[2],n[1]-n[3]);
int e=0xff&q,r=0xff&w;

while(true)
{
Console.ReadLine();
Console.WriteLine((--r<0?"":w<0?"N":"S")+(--e<0?"":q<0?"W":"E"));
}
}
}