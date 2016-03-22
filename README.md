# vActiveRecord

Module for C#

# Getting Started

## How to use

```
using vActiveRecord.Base;

namespace Application
{
	public class People : vActiveRecord.Base
	{
		...
	}
}
```

## Connection

```
vActiveRecord.Base.establish_connection(SQLITE_FULL_PATH);
```

## Methods

### public List<Hashtable> all()

```
People people = new People();
List<Hashtable> allRows = people.all();

```

### public find(Hashtable args)

```
People people = new People();

Hashtable args = new Hashtable();
args.Add("conditions", "first_name='John' AND age>=18");
args.Add("order", "name DESC");
args.Add("select", "first_name,last_name,age");
//args.Add("group", "last_name");
//args.Add("limit", "100");
//args.Add("offset", "50");
//args.Add("joins", "AS p INNER JOIN city AS c ON p.city_id=c.id");

p.find(args)
```
