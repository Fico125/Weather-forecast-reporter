# Windows service used to retrieve & report weather data.

- Weather data is retrieved using OpenWeatherMap API in combination with the lattitude and longitude of the desired place.
- Retrieved data is stored into an MSSQL Express database.
- Service implements caching mechanism so reduce the need for accessing underlying slower storage layer.
- Service reports location, timestamp, average temperature and the biggest temperature oscillation in Event Viewer.

To install the service: 
1. Build the solution.
2. Copy the bin/Debug output to a desired location where you'd like to store the service files.
3. Adjust WeatherReport.exe.config file:

3.1 Change latitude & longitude key values to match your desired location.
```
		<add key="latitude" value="45.22" />
		<add key="longitude" value="13.59" />
```
3.2 Change connectionString values and adjust the SQL server name, and the database which will hold weather report tables.
```
		<connectionStrings>
			<add name="DatabaseConnection" providerName="System.Data.SqlClient"
				  connectionString="Server=YOUR_SERVER_NAME;Database=YOUR_DATABASE_NAME;Trusted_Connection=True" />
		</connectionStrings>
```
	3.3 Change API_KEY value to match your OpenWeatherMap API key.
```
		<add key="API_KEY" value="YOUR_API_KEY_VALUE" />
```
3. Open Windows Powershell as an administrator.
4. Type: 
```bash
WeatherReport.exe -install
```
5. To uninstall the service type: 
```bash
WeatherReport.exe -uninstall
```
