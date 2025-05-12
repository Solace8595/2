@section Scripts {
    <script>
        function submitOrder() {
            const form = document.getElementById('checkoutForm');
        const formData = new FormData(form);
        const order = { };
        for (let [key, value] of formData.entries()) {
            order[key] = value;
            }

        fetch('/Cart/SubmitOrder', {
            method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
        body: JSON.stringify(order)
            }).then(response => {
                if (response.ok) {
            location.href = '/Cart/Success';
                } else {
            alert("Ошибка при оформлении заказа");
                }
            });
        }
    </script>
    @Html.AntiForgeryToken()
}
