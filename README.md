# N-Tier Caching Strategies in .NET Core

### Straight to the Code? 

If you're impatient and just want to see the action, go [here](docs/GettingStarted.md) to get started

### Caching in .NET Core

#### Caching Strategies for .NET Core:

Performance is one of the biggest considerations when it comes to enterprise applications. It doesn't matter how useful your application is, if the response times are far below the user expectations, they will be unhappy. And when users are unhappy, chances are they will migrate to other platforms, even if those platforms are not as feature rich. 

Caching your most frequently used data is one of the easiest ways to speed up your applications. By temporarily storing the results of heavy computations and other slow calls, your specific service is able to response much faster, since the data it needs can be accessed much faster. In general caches fall into two categories: distributed caches and in memory caches.

#### Distributed Cache:

In distributed caching, your caching is generally done out of process using a dedicated caching platform, such as Redis. This allows for all the niceties that we're used to in enterprise applications, such as being able to scale horizontally and being able to increase your transactional capacity.

The biggest advantage of the distributed cache is that it's shared by all the instances in your server farm. That makes it easier for cache invalidation and only one worker instance needs to perform or respond to updates if required. 

However, one of the main pitfalls of distributed caches that most developers seem to forget is that, at the end of the day, they are still network calls. That means that all the standard caveats due to bandwidth and network latency still apply. This quickly adds up when you have to make multiple of these calls, especially with providers such as Redis which don't really allow for batched requests. 

#### In Memory Cache:

This is where in memory caches shine. In in memory caching, the data is stored in process alongside the currently executing code. That means that the data is always instantly ready with constant time lookups. However, this also introduces a huge problem. Now, each individual worker instance is maintaining a separate copy of the desired item. This makes cache invalidation and updates trickier. 

One good way we've found to circumvent this is to use message brokers (Kafka in our particular instance). When an interesting event occurs that requires a cache invalidation or update, the event is broadcast to the individual worker instances. This allows them to remain synced and up to date. One caveat of this approach is the increased chattiness. First, for broadcasting the event, then for each individual instance to make whatever other network calls they need to make in order to keep their caches up to date. 

#### N-Tier Caching:

Obviously, things do get more complicated in a microservice environments. In these environments, it's not uncommon to call one microservice, that then calls other microservices and so on, with the results being aggregation by the intermediate services as they make their way back up the stack. 