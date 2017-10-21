# IqCommander

I needed a way to pull historical data from iqfeed and put it into a postgres database. On the way there I thought it would
also be handy to have a REPL for sampling the iqfeed data and trying out various command parameters.

The result is reasonably useful at this time. Historical Tick data is supported, Interval data is not supported yet but will
be added soon.

To use this you will need to create an app.config file (modify app.example.config):

    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
       <appSettings>
           <add key="Product" value="[product id]" />
           <add key="Version" value="[product version]" />
           <add key="PostgresUser" value="[postgres username]" />
           <add key="PostgresPassword" value="[postgres password" />
           <add key="PostgresPort" value="[postgres port]" />
           <add key="PostgresDatabase" value="[postgres iqfeed]" />
       </appSettings>
    </configuration>

Postgres usage assumes a specific table format, see source.

