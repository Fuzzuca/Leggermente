using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leggermente.lib;
using Leggermente.lib.MATE;

namespace Leggermente.lib.GIGI{ class GIGI{

static const int _num0 = new Variable(_num0, 0);
static const int _num1 = new Variable(_num1, 10);
static const int _num2 = new Variable(_num2, 0);
static const int _num3 = new Variable(_num3, 10);
static const int _num4 = new Variable(_num4, 0);
static const int _num5 = new Variable(_num5, 10);
static const int _num6 = new Variable(_num6, 0);
static const int _num7 = new Variable(_num7, 1);

static Variable crea(){
	Variable num1 = new Variable("num1",(MATE.random((_num0),(_num1))));
	Variable num2 = new Variable("num2",(MATE.random((_num2),(_num3))));
	Variable num3 = new Variable("num3",(MATE.random((_num4),(_num5))));
	num1[_num6].ChangeValue((_num7).ToString());
	return new Variable("NULL",null);
}

} }
