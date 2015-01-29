WSD.Rest
---

Simple library to work with api servers

Setup
---

To setup we need to initialize the lib:

```
Client.Setup ("http://our.production.url", "http://our.dev.url", "/v1", "x-application-id", "x-application-secret", "x-access-token");
Client.Init ("application id", "application secret");
```

**Client.Setup** (params)
- Production URL
- Development URL, yes it detects when you are using Debug or Release your app
- API version
- Header for application id, is used when you has one server for more than one app, in the case that you are building a BaaS or something else
- Header for application secret
- Header for application access token, when you app works with authentication

**Client.Init** (params)
- Application id
- Application secret

Usage
---

**GET**

```
// Just a common query, this will be converted to querystring
Query query = new Query ();
query.Search = "A search them";
query.Offset = 0;
query.Limit = 50;
query.Order = "id desc";

Response response = await Client.Get ("/post", query);

string content = response.Content;

// Also can cast to an object

List<Post> posts = response.Get<List<Post>> ();
```

**POST / PUT**

```
Dictionary<string, object> data = new Dictionary<string, object> () {
	{ "Title", "Post title" },
	{ "Content", "Post content" }
};

Response response = await Client.Post ("/post", null, data);

Post post = response.Get<Post> ();

Dictionary<string, object> data = new Dictionary<string, object> () {
	{ "Title", "Edited title" },
	{ "Content", "Edited content" }
};

Response response = await Client.Put ("/post", "1", data);

Post post = response.Get<Post> ();
```

**DELETE**

```
Response response = await Client.Delete ("/post", 1);

string content = response.Content;
```

**Query**

You can extend it to put your custom query string attributes

```
public class CustomQuery : Query
{
	public string CustomField { get; set; }
}

CustomQuery query = new CustomQuery();
query.CustomField = "Custom data";

await Client.Get ("/api-endpoint", query);
```

**File**

Ease the file upload

```
byte[] fileContents = ....; // it must be implemented in each platform no PCL way yet
Dictionary<string, object> data = new Dictionary<string, object> () {
	{ "Title", "Edited title" },
	{ "Content", "Edited content" },
	{ "Image", new File("FileName.png", "image/png", fileContents) }
};

Response response = await Client.Put ("/post", "1", data);

Post post = response.Get<Post> ();
post.Image.Url; // access the url of the file
```

Todo
---

Better documentation