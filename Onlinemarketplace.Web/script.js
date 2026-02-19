document.addEventListener("DOMContentLoaded", () => {
    const chooseMenu = document.getElementById("chooseMenu");
    const currentMenu = document.getElementById("currentMenu");
    const currentMethods = document.getElementById("currentMethods");
    const menuIcon = document.getElementById("menuIcon");
    const allProducts = document.getElementById("allProducts");


    const localhost = "https://localhost:7119/";


    let menus = ["Authentication", "Marketplace", "Admin"];


    const api = {
        Authentication: {
            register: {
                method: "POST",
                body: ["username", "email", "password"]
            },
            login: {
                method: "POST",
                body: ["username", "email", "password"]
            }
        },

        Marketplace: {
            "": {
                method: "GET",
            },
            order: {
                method: "POST",
                body: ["productsId"]
            },
            getAccountInfo: {
                method: "GET"
            },
            getLogs: {
                method: "GET"
            }
        },

        Admin: {
            products: {
                method: "GET"
            },
            "": {
                method: "POST",
                body: ["name", "price", "inStock"]
            },
            updateProduct: {
                method: "PATCH",
                id: true,
                body: ["name", "price", "inStock", "isAvailable"]
            },
            deleteProduct: {
                method: "DELETE",
                id: true,
            },
            users: {
                method: "GET"
            },
            getOrdersInShoppingCart: {
                method: "GET"
            },
            getOrdersPurchased: {
                method: "GET"
            },
            changeOrderStatus: {
                method: "PATCH",
                body: ["orderId", "status"]
            },
            changeAccountBalance: {
                method: "PATCH",
                body: ["accountId", "newBalance"]
            }
        }
    }

    localStorage.clear();
    




    // ------------------------- Functions -------------------------
    function Clear(obj) {
        obj.innerHTML = "";
    }
    

    currentMenu.textContent = "Select a menu";
    currentMenu.className = "text borderRadius";


    function getProductImage(name) {
        const productName = name.toLowerCase();
        
        if (productName.includes("iphone")) {
            return "https://store.storeimages.cdn-apple.com/4982/as-images.apple.com/is/iphone-15-pro-finish-select-202309-6-7inch-naturaltitanium?wid=5120&hei=2880&fmt=p-jpg&qlt=80&.v=1692845702708";
        }
        
        if (productName.includes("samsung")) {
            return "https://files.refurbed.com/ii/samsung-galaxy-s24-ultra-1705563115.jpg";
        }

        if (productName.includes("headphones")) {
            return "https://i-store.net/_sh/73/7328.jpg";
        }

        if (productName.includes("laptop")) {
            return "https://elmir.ua/img/og-p-91456/0/0/elmir.jpg";
        }

        return "https://ichef.bbci.co.uk/ace/standard/976/cpsprodpb/14235/production/_100058428_mediaitem100058424.jpg";
    }


    function LoadMethods(currentMenu) {
        const endpoints = Object.keys(api[currentMenu]);

        endpoints.forEach(endpoint => {
            const listItem = document.createElement("div");

            listItem.textContent = endpoint || "(root)";
            listItem.className = "method-item";

            currentMethods.append(listItem);

            listItem.addEventListener("click", async() => {
                document.querySelectorAll(".method-item").forEach(element => element.classList.remove("active"));

                listItem.classList.add("active");

                await HandleEndpointClick(currentMenu, endpoint);
            })
        })
    }

    async function CallApi(controller, endpoint, data = {}, id = null) {
        const config = api[controller][endpoint];

        let url = localhost + controller;

        if (endpoint) url += "/" + endpoint;

        if (config.id && id) url += "/" + id;


        const options = {
            method: config.method,
            headers: {
                "Content-Type": "application/json",
                "Authorization": localStorage.getItem("token")
                    ? "Bearer " + localStorage.getItem("token")
                    : ""
            }
        };

        if (config.body) options.body = JSON.stringify(data);
        
        const result = await fetch(url, options);

        const text = await result.text();

        if (!result.ok) throw new Error(`${result.status}: ${text}`);

        const responseData = text ? JSON.parse(text) : {};

        if (endpoint === "login" && responseData.token) {
            localStorage.setItem("token", responseData.token);
        }

        return responseData;
    }

    async function HandleEndpointClick(controller, endpoint) {
        const config = api[controller][endpoint];

        let data = {};
        let id = null;

        if (config.id) {
            id = prompt("Enter ID: ");
            if (!id) return;
        }

        if (config.body) {
            for (const field of config.body) {
                const value = prompt(`Enter ${field}`);

                if (value === null) return;

                if (field === "productsId") {
                    data[field] = value.split(',').map(item => Number(item.trim()));
                }
                else if (value.toLowerCase() === "true" || value.toLowerCase() === "false") {
                    data[field] = (value.toLowerCase() === "true");
                }
                else if (!isNaN(value) && value.trim() !== "") {
                    data[field] = Number(value);
                }
                else {
                    data[field] = value;
                }
            }
        }

        try {
            const result = await CallApi(controller, endpoint, data, id);
            console.log("RAW:", result);

            console.log("Result: ", result);


            if (controller === "Authentication" && endpoint === "register") {
                ShowRegisterResult(true, result.message || "Registration successful");
                return;
            }


            if (controller === "Authentication" && endpoint === "login") {
                ShowLoginInfo(result);
                return;
            }


            if (controller === "Marketplace" && endpoint === "") {
                GetAllProducts(result);
                return;
            }

            if (controller === "Marketplace" && endpoint === "getAccountInfo") {
                ShowAccountInfo(result);
                return;
            }


            if (controller === "Admin" && endpoint === "products") {
                ShowAdminProducts(result);
                return;
            }


            if (controller === "Admin" && endpoint === "users") {
                ShowAdminUsers(result);
                return;
            }

            if (controller === "Admin" && endpoint === "getOrdersInShoppingCart") {
                ShowAdminOrders(result, "Shopping Cart");
                return;
            }

            if (controller === "Admin" && endpoint === "getOrdersPurchased") {
                ShowAdminOrders(result, "Purchased Orders");
                return;
            }

            if (controller === "Marketplace" && endpoint === "getLogs") {
                ShowLogs(result);
                return;
            }

            alert(JSON.stringify(result, null, 2));
        }
        catch (error) {
            console.error(error);

            if (controller === "Authentication" && endpoint === "register") {
                ShowRegisterResult(false, error.message);
                return;
            }

            alert("Error " + error);
        }
    }


    function ShowRegisterResult(success, message) {
        allProducts.innerHTML = "";

        const register = document.createElement("div");
        register.id = "registerResult";

        if (success) {
            register.className = "register-success";
            register.textContent = "✅ " + message;
        } 
        else {
            register.className = "register-error";
            register.textContent = "❌ " + message;
        }

        allProducts.append(register);
    }



    function GetAllProducts(productsData) {
        let list = [];

        if (Array.isArray(productsData.products)) {
            list = productsData.products;
        } 
        else {
            console.error("Unknown format:", productsData);
            return;
        }

        const products = document.createElement("div");
        products.id = "products";

        list.forEach(element => {
            const product = document.createElement("div");
            product.className = "product";

            const image = document.createElement("img");
            image.src = element.imageUrl || getProductImage(element.name);

            const name = document.createElement("div");
            name.textContent = element.name;

            const price = document.createElement("div");
            price.textContent = "Price: " + element.price;

            const inStock = document.createElement("div");
            inStock.textContent = "In stock: " + element.inStock;

            product.append(image, name, price, inStock);
            products.append(product);
        });

        allProducts.innerHTML = "";
        allProducts.append(products);
    }


    function ShowAccountInfo(data) {
        allProducts.innerHTML = "";

        const info = document.createElement("div");
        info.id = "accountInfo";

        const title = document.createElement("h2");
        title.textContent = "Account Info";

        info.append(title);

        function addRow(label, value) {
            const row = document.createElement("div");
            row.className = "account-row";

            const left = document.createElement("span");
            left.textContent = label;

            const right = document.createElement("span");
            right.textContent = value ?? "-";

            row.append(left, right);
            info.append(row);
        }

        addRow("ID", data.id);
        addRow("Username", data.username);
        addRow("Email", data.email);
        addRow("Balance", data.balance);
        addRow("Role", data.role);

        allProducts.append(info);
    }


    function ShowLoginInfo(data) {
        allProducts.innerHTML = "";

        const box = document.createElement("div");
        box.id = "loginInfo";

        const title = document.createElement("h2");
        title.textContent = "Login Result";

        box.append(title);

        
        const status = document.createElement("div");

        if (data.token) {
            status.textContent = "Login successful ✅";
            status.className = "login-success";
        } 
        else {
            status.textContent = "Login failed ❌";
            status.className = "login-error";
        }

        box.append(status);



        if (data.message) {
            const msg = document.createElement("div");
            msg.textContent = "Message: " + data.message;

            box.append(msg);
        }



        if (data.token) {
            const tokenTitle = document.createElement("div");
            tokenTitle.textContent = "JWT Token:";

            const tokenBox = document.createElement("div");
            tokenBox.className = "token-box";
            tokenBox.textContent = data.token;

            box.append(tokenTitle, tokenBox);
        }

        allProducts.append(box);
    }


    function ShowAdminProducts(productsData) {
        allProducts.innerHTML = "";

        const container = document.createElement("div");
        container.id = "adminProducts";

        productsData.forEach(p => {
            const product = document.createElement("div");
            product.className = "admin-product";

            const image = document.createElement("img");
            image.src = p.imageUrl || getProductImage(p.name);

            const id = document.createElement("div");
            id.innerHTML = `<span class="field">ID:</span> ${p.id}`;

            const name = document.createElement("div");
            name.innerHTML = `<span class="field">Name:</span> ${p.name}`;

            const price = document.createElement("div");
            price.innerHTML = `<span class="field">Price:</span> ${p.price}`;

            const inStock = document.createElement("div");
            inStock.innerHTML = `<span class="field">In Stock:</span> ${p.inStock}`;

            const isAvailable = document.createElement("div");
            isAvailable.innerHTML = `<span class="field">Available:</span> ${p.isAvailable}`;

            product.append(image, id, name, price, inStock, isAvailable);
            container.append(product);
        });

        allProducts.append(container);
    }


    function ShowAdminUsers(data) {
        allProducts.innerHTML = "";

        const container = document.createElement("div");
        container.id = "adminUsers";


        let allUsersList = [];
        
        if (data.admins) {
            data.admins.forEach(a => allUsersList.push({ ...a, roleType: "Admin" }));
        }
        if (data.customers) {
            data.customers.forEach(c => allUsersList.push({ ...c, roleType: "Customer" }));
        }

        allUsersList.forEach(user => {
            const card = document.createElement("div");
            card.className = "admin-user";

            const id = document.createElement("div");
            id.innerHTML = `<span class="field">ID:</span> ${user.id}`;

            const username = document.createElement("div");
            username.innerHTML = `<span class="field">Username:</span> ${user.userName}`;

            const email = document.createElement("div");
            email.innerHTML = `<span class="field">Email:</span> ${user.email}`;

            const balance = document.createElement("div");
            balance.innerHTML = `<span class="field">Balance:</span> ${user.balance}`;

            const role = document.createElement("div");
            role.innerHTML = `<span class="field">Role:</span> ${user.roleType}`;

            card.append(id, username, email, balance, role);
            container.append(card);
        });

        allProducts.append(container);
    }


    function ShowAdminOrders(orders, titleText) {
        allProducts.innerHTML = "";

        const title = document.createElement("h2");
        title.textContent = titleText;
        title.style.margin = "1rem";
        allProducts.append(title);

        const container = document.createElement("div");
        container.id = "adminOrders";

        orders.forEach(order => {
            const card = document.createElement("div");
            card.className = "admin-order-card";


            let statusClass = "status-default";
            if (order.status === "InShoppingCart") statusClass = "status-cart";
            if (order.status === "Purchased") statusClass = "status-purchased";
            if (order.status === "Cancel") statusClass = "status-cancelled";

            card.innerHTML = `
                <div class="order-header">
                    <span class="field">Order ID:</span> ${order.id}
                </div>
                <div class="order-body">
                    <div><span class="field">User ID:</span> ${order.userId}</div>
                    <div><span class="field">Total Price:</span> <span class="price-tag">${order.totalPrice} $</span></div>
                    <div><span class="field">Products IDs:</span> <span class="products-list">${order.products}</span></div>
                    <div class="status-badge ${statusClass}">${order.status}</div>
                </div>
            `;
            container.append(card);
        });

        allProducts.append(container);
    }


    function ShowLogs(logsData) {
        allProducts.innerHTML = "";

        const container = document.createElement("div");
        container.id = "logsContainer";

        const levelNames = ["DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL"];
        const sorted = [...logsData].reverse();

        sorted.forEach(log => {
            const level = levelNames[log.logLevel] || "INFO";

            const row = document.createElement("div");
            row.className = "log";

            row.innerHTML = `
                <div class="log-time">${log.date}</div>
                <div class="log-status">${level}</div>
                <div class="log-text">
                    ${log.description || ""}
                </div>
            `;

            container.append(row);
        });

        allProducts.append(container);
    }



    // -------------------------------------------------------------


    const content = document.getElementById("content");



    menuIcon.addEventListener("click", () => {
        chooseMenu.classList.toggle("active");
        content.classList.toggle("shifted");

        currentMenu.classList.toggle("shifted");

        if (chooseMenu.classList.contains("active")) {
            Clear(chooseMenu);

            menus.forEach(menuName => {
                const controller = document.createElement("div");

                controller.textContent = menuName;
                controller.className = "point";
                controller.style.color = "white";

                controller.style.marginBottom = "1rem";
                controller.style.fontSize = "1.2rem";

                chooseMenu.append(controller);

                controller.addEventListener("click", () => {
                    controller.className = "point";
                    currentMenu.textContent = controller.textContent;
                    
                    Clear(currentMethods);
                    LoadMethods(currentMenu.textContent);
                })
            });
        }
    });
});