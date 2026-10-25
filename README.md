Simulator (app1) --> SignalR Hub --> Redis Pub/Sub backplane --> Hub (app2)
                         |                                          |
                         v                                          v
                  Browser clients                            Browser clients
                         |
                         +--> Redis cache (last-known positions)