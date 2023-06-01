namespace SwishVending.MachineUI.Data
{
    public class VendingMachineService
    {
        private int vendingMachineId = 1;
        public Task<string[]> GetForecastAsync()
        {
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => "test").ToArray());
        }

        public async Task<string> GetQRCode(int idProduct)
        {
            var message = $"{vendingMachineId},{idProduct}";
            var client = new HttpClient();
            var message2 = await client.PostAsync("http://localhost:7070", null);
            return null;
        }
    }
}