---
title: Working with the Project and Adding New Features using Visual Studio for Mac
parent: Walkthroughs
nav_order: 9
---

## New Feature: Improved Order Status

Let's add a new feature to the site that will allow authenticated users to view the current status of their orders. Orders should display their status on the My Orders page and the Order Details Page. Order Status should have the following options:

- Pending
- Out for Delivery
- Delivered

The current version of the application lists "Pending" for every order - it's hard coded in the `OrderController` class for both the My Orders list action and the order details action.

The My Orders action should simply list the correct status. The details page should display the status as well as a (simply calculated from order date) estimate of when the order will be delivered. Delivered orders should display the (calculated) date when they were delivered.

## Update the UI

We can start on this feature from either end, working from the domain model first or from the UI first. In this case, since there's not much domain logic involved and the UI is pretty straightforward, let's start with the UI.

The My Orders page should display the current order status of each order, and should link to the order details page. This is already done, so there's nothing we need to change in MyOrders.cshtml.

![image](https://github.com/user-attachments/assets/6b1948c6-2efe-404b-ac55-a00e39796d40)

The Order Details page currently just displays the order status, but not delivery estimate or actual delivery time information. The view uses this markup to display the status:

`<section class="esh-orders-detail-item col-xs-3">@Model.Status</section>`

In this scenario, we can add the date information directly to the model, so that when it displays it's something like this:

```text
Out for Delivery - ETA 1645
or
Delivered today at 1645
```

It's best to minimize logic in views and viewmodels, so we'll add the logic for these messages to the controller for now. Since all we have to work with at the moment are hard-coded status strings, we can create a simple helper method that uses a string as input, like this one:

```csharp
private string GetDetailedStatus(string status)
{
    if (status == "Pending") return status;
    if (status == "Out for Delivery")
    {
        return $"{status} - ETA {DateTime.Now.AddHours(1).ToShortTimeString()}";
    }
    if (status == "Delivered")
    {
        return $"{status} at {DateTime.Now.AddHours(-1).ToShortTimeString()}";
    }
    return "Unknown";
}
```

**Note: You'll need to add `using System;`**

Now all we need to do is replace the place were `Status` is being set in the viewmodel in the Detail action with a call to this method:

```csharp
        Status = GetDetailedStatus("Delivered"),
```

Now we can test the UI and verify our message displays as we expect:

![image](https://github.com/user-attachments/assets/e8a03361-d401-4617-86ac-004f4dcf1ace)

## Update the Model

In order to populate the ViewModel, we'll need to get the order status information from our domain model. So, we'll add that information. Go to the Order class in ApplicationCore - Entities - OrderAggregate, and add a new string property, Status. We can tie this to an enum or a separate entity in a future update, but for now we'll just use a string.

```csharp
        public string Status { get; private set; } = "Pending";
```

## Update the Database

Since we're using the EF Core InMemory database, we don't need to update the database just yet. When we're ready to test this with a real database, or before going to production, we would add new migrations using the instructions in [the project README](https://github.com/NimblePros/eShopOnWeb/blob/master/README.md) and then call dotnet ef database update with the appropriate arguments to apply the changes.

## Setting the Values

Currently there is no logic to set the order status after it's been created, so it will always be "Pending". A real application would likely get updates from some external service based on notifications or events sent to it by a shipping provider. For this demo, we'll just set the status when the order is created, so we can see that our changes to the UI work correctly with data from the model.

Orders are created in the ApplicationCore - Services - OrderService class. We can set the status right after the order is created, but before it's added to the repository:

```csharp
     var order = new Order(basket.BuyerId, shippingAddress, items);

     await _orderRepository.AddAsync(order);
```

Note that we made the Status property read only by using a private set method. We want to promote encsapsulation and we don't want to allow any part of our application to set the status to any value desired. Instead, we are going to expose two methods that will update the status to specific values ("Out for Delivery" and "Delivered"). Update the Order class by adding these two methods:

```csharp
        public void SetStatusOutForDelivery()
        {
            Status = "Out for Delivery";
        }

        public void SetStatusDelivered()
        {
            Status = "Delivered";
        }
```

Now all that's left is to call these methods some of the time before saving new orders. We can do that based on the system clock just for demo purposes:

```csharp
        int currentSecond = DateTime.Now.Second;
        if(currentSecond % 3 == 1)
        {
            order.SetStatusOutForDelivery();
        }
        if(currentSecond % 3 == 2)
        {
            order.SetStatusDelivered();
        }
```

**Note: This requires `using System;`**

This will update the status from Pending 2/3 of the time based on the value of the "seconds" component of the current time (0-59).

## Back to the UI

Now that we have a Status property on Order, we need to use it in our UI code. Back in the OrderController class we need to set the Status property on the viewmodel to the Status property on the order, both in the MyOrders action and in the Detail action.

```csharp
        Status = o.Status,
```

Now we can test the application by adding a bunch of orders and checking their order statuses:

![image](https://github.com/user-attachments/assets/e5733e38-2600-4b5d-bc96-2841996cf276)

Individual Order Details views display the correct information as well:

![image](https://github.com/user-attachments/assets/d68c429f-9555-4957-a3f3-3128e9700aed)

## Wrapping up

A good idea before calling this feature complete would be to add unit tests. We could easily add unit tests for the Order entity that verify the following at minimum:

* New orders have a Status and its value is Pending
* SetStatusOutForDelivery sets Status to "Out for Delivery"
* SetStatusDelivered sets Status to "Delivered"

In addition, we could write integration tests using a test database instance that would confirm that the Status property of orders is properly saved and retrieved from the database (after we run migrations). We also could add functional tests that would confirm the status was displayed as expected on the My Orders and Order Detail pages. With these tests in place, we could be confident not only that the system works as designed today, but by running these tests as part of our build pipeline, we would also be confident that this behavior remained working through future updates without the need to manually retest it.