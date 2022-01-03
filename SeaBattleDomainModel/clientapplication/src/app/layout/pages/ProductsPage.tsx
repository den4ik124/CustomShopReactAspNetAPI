import { observer } from "mobx-react-lite";
import React, { Fragment, useEffect, useState } from "react";
import { Item, Button, Label,  Container, Header, Card, Icon, Image, Grid} from "semantic-ui-react";
import agent from "../../api/agent";
import DeleteButton from "../../common/DeleteButton";
import EditButton from "../../common/EditButton";
import { OrderItem } from "../../models/orderItem";
import { Product } from "../../models/product";
import { useStore } from "../../stores/store";
import LoadingComponent from "../components/LoadingComponents";
import CreateNewProduct from "./Modals/CreateNewProduct";

function ProductsPage(){
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const {userStore} = useStore()
    const {productStore} = useStore()
    const [shouldUpdate, setUpdateList] = useState(false);
    const {orderItemStore} = useStore();


    if(userStore.isLoggedIn){
        var user =  userStore.user;
    }

    useEffect(() => {
        agent.Products.list().then(response => {
            setProducts(response);
          setLoading(false);
          setUpdateList(false)
        })
    }, [shouldUpdate])

function renderControllButtons(product : Product){

    if(user!.roles.includes('Manager') || user!.roles.includes('Admin')){
        return(
            <>
                <DeleteButton floated="right" onClick={() => handleRemove(product.id)}/>
                <EditButton floated='right' onClick={() => null}/>
            </>
        )
    }
}

    console.log(products);

    if(loading) return <LoadingComponent content="Loading products..."/>

    function handleRemove(id: string){
        console.log('Product has been removed...');
        productStore.removeProduct(id);
        setUpdateList(true);
    }

    function handleProductBuying(product: Product){
        orderItemStore.createOrderItem(product);
    }

return(
    <Fragment>
        <Label ribbon color="red" size="huge" content="Page is in design progress ..."/>
        {user!.roles.includes('Manager') || user!.roles.includes('Admin') ? (
            <>
                <CreateNewProduct trigger={<Button fluid positive content="Add new product"
                style={{marginBottom: "50px"}} />}/>
            </>
        ) : null}

        <Grid columns={4} relaxed stackable>
        {products.map((product) => (
            <Grid.Column key={product.id}>
                <Card>
                    <Image 
                        rounded
                        wrapped
                         style={{marginRight: "50px"}} 
                        size='medium' 
                        src={`/sources/img/products/${product.title}.png`}
                    />
                    
                    <Card.Content textAlign="left">
                        <Card.Header>
                            {product.title}
                            {renderControllButtons(product)}

                        </Card.Header>
                        <Card.Meta>
                            <span className='date'>Joined in 2015</span>
                        </Card.Meta>
                        <Card.Description>
                            {product.description}
                            Lorem ipsum dolor sit amet consectetur adipisicing elit. Quam, distinctio sed ipsa rem unde minus enim quasi id ipsam iusto nisi eum sapiente. Sint sapiente rem voluptatibus eos nobis sequi.
                        </Card.Description>
                    </Card.Content>
                    <Card.Content extra>
                        <Header 
                            textAlign="center"
                            color="green"
                            content={product.price + ' UAH'}
                        />
                        <Button 
                        fluid
                            positive 
                            position="right" 
                            content='Buy now!'
                            onClick={() => handleProductBuying(product)}
                        />
                    </Card.Content>
                </Card>
        </Grid.Column>
        ))}
        </Grid>
    </Fragment>
)
}

export default observer(ProductsPage);