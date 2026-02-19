import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface WeatherForecast {
  time: string;
  condition: string;
  temp: number;
  irradiance: number;
}

interface WeatherData {
  current: {
    temp: number;
    humidity: number;
    wind: number;
    condition: string;
    irradiance: number;
  };
  forecast: WeatherForecast[];
}

const weatherData: WeatherData = {
  current: {
    temp: 24,
    humidity: 45,
    wind: 12,
    condition: "Partly Cloudy",
    irradiance: 680,
  },
  forecast: [
    { time: "12:00", condition: "Sunny", temp: 26, irradiance: 850 },
    { time: "15:00", condition: "Sunny", temp: 28, irradiance: 720 },
    { time: "18:00", condition: "Cloudy", temp: 23, irradiance: 180 },
    { time: "21:00", condition: "Clear", temp: 19, irradiance: 0 },
  ],
};

export function WeatherWidget() {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-base">Weather</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-3">
          <div className="flex items-baseline justify-between">
            <div>
              <span className="text-3xl font-semibold">
                {weatherData.current.temp}
              </span>
              <span className="text-lg text-muted-foreground">°C</span>
            </div>
            <span className="text-sm text-muted-foreground">
              {weatherData.current.condition}
            </span>
          </div>

          <div className="grid grid-cols-3 gap-2 text-sm">
            <div>
              <p className="text-muted-foreground text-xs">Humidity</p>
              <p className="font-medium">{weatherData.current.humidity}%</p>
            </div>
            <div>
              <p className="text-muted-foreground text-xs">Wind</p>
              <p className="font-medium">{weatherData.current.wind} km/h</p>
            </div>
            <div>
              <p className="text-muted-foreground text-xs">Irradiance</p>
              <p className="font-medium text-energy-solar">
                {weatherData.current.irradiance} W/m²
              </p>
            </div>
          </div>
        </div>

        <div className="border-t pt-3">
          <p className="text-xs text-muted-foreground mb-2">Forecast</p>
          <div className="space-y-2">
            {weatherData.forecast.map((item) => (
              <div
                key={item.time}
                className="flex items-center justify-between text-sm"
              >
                <span className="text-muted-foreground w-12">{item.time}</span>
                <span className="flex-1">{item.condition}</span>
                <span className="w-10 text-right">{item.temp}°</span>
                <span className="w-16 text-right text-xs text-muted-foreground">
                  {item.irradiance > 0 ? `${item.irradiance} W/m²` : "-"}
                </span>
              </div>
            ))}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
