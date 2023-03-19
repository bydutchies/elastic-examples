using Nest;

namespace MyElastic.IntegrationTests;

[ElasticsearchType(IdProperty = nameof(Id))]
public class Post
{
  public int Id { get; set; }

  [Text(Boost = 1.5)]
  public string Title { get; set; }

  [Nest.Ignore]
  public string Url { get; set; }

  public string Body { get; set; }

  public string Author { get; set; }
}
