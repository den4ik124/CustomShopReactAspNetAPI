import { observer } from "mobx-react-lite";
import React, { Fragment, SyntheticEvent, useEffect, useState } from "react";
import { Item, Button, Label,  Container, Header, Card, Icon, Image, Grid} from "semantic-ui-react";
import agent from "../../api/agent";
import BuyProductButton from "../../common/BuyProductButton";
import DeleteButton from "../../common/DeleteButton";
import EditButton from "../../common/EditButton";
import { OrderItem } from "../../models/orderItem";
import { Product } from "../../models/product";
import { useStore } from "../../stores/store";
import LoadingComponent from "../components/LoadingComponents";
import CreateNewProduct from "./Modals/CreateNewProduct";
import EditProductItem from "./Modals/EditProductItem";

function ProductsPage(){
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const [shouldUpdate, setUpdateList] = useState(false);
    const [disabled, setDisabled] = useState(false);
    const [target, setTarget] = useState('');

    const {userStore} = useStore()
    const {productStore} = useStore()
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
                <DeleteButton name={product.id} floated="right" onClick={() => handleRemove(product.id)}/>
                <EditProductItem 
                    trigger={<EditButton floated='right'/>}
                    product = {product}/> 
                {/* <EditButton floated='right' onClick={() => <EditProductItem trigger/>}/> */}
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

    function handleProductBuying(e: SyntheticEvent<HTMLButtonElement>, product: Product){
        setTarget(e.currentTarget.name)
        orderItemStore.createOrderItem(product);
        setDisabled(true);
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
                            <span className='date'>Some additional information</span>
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
                        {/* <BuyProductButton name={product.id} onClick={() => handleProductBuying(product)}/> */}
                        <Button 
                            name={product.id}
                            //disabled={disabled}
                            fluid
                            positive 
                            icon={<Icon name="shop"/>}
                            position="right" 
                            content='Buy now!'
                            onClick={(e) => handleProductBuying(e, product) }
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